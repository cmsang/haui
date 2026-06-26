using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Haui.PCB.ViewModels;

/// <summary>One row in the YOLO class names editor.</summary>
public sealed class ClassNameItem : INotifyPropertyChanged
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
    private bool _showInspectionResultAfterRecognition;
    private string _com = "COM3";
    private string _warehouseCom = string.Empty;
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private double _jogStepDegrees = 10;
    private int _speedPercent = 50;
    private string _databaseConnection = string.Empty;
    private string _detectionModelPath = ComponentDetectionSettings.DefaultModelPath;
    private int _detectionInputWidth = ComponentDetectionSettings.DefaultInputWidth;
    private int _detectionInputHeight = ComponentDetectionSettings.DefaultInputHeight;
    private double _detectionConfThreshold = ComponentDetectionSettings.DefaultConfThreshold;
    private double _detectionIouThreshold = ComponentDetectionSettings.DefaultIouThreshold;
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

    public ObservableCollection<ClassNameItem> ClassNames { get; } = [];

    public bool DeveloperMode
    {
        get => _developerMode;
        set
        {
            if (_developerMode == value) return;
            _developerMode = value;
            OnPropertyChanged();
        }
    }

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

    public string DetectionModelPath
    {
        get => _detectionModelPath;
        set { _detectionModelPath = value; OnPropertyChanged(); }
    }

    public int DetectionInputWidth
    {
        get => _detectionInputWidth;
        set { _detectionInputWidth = value; OnPropertyChanged(); }
    }

    public int DetectionInputHeight
    {
        get => _detectionInputHeight;
        set { _detectionInputHeight = value; OnPropertyChanged(); }
    }

    public double DetectionConfThreshold
    {
        get => _detectionConfThreshold;
        set { _detectionConfThreshold = value; OnPropertyChanged(); }
    }

    public double DetectionIouThreshold
    {
        get => _detectionIouThreshold;
        set { _detectionIouThreshold = value; OnPropertyChanged(); }
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
        ShowInspectionResultAfterRecognition = setting.ShowInspectionResultAfterRecognition;
        Com = setting.Com;
        WarehouseCom = setting.WarehouseCom;
        BaudRate = setting.BaudRate;
        StepsPerDeg = setting.StepsPerDeg;
        JogStepDegrees = setting.JogStepDegrees;
        SpeedPercent = setting.SpeedPercent;
        DatabaseConnection = setting.DatabaseConnection;

        var detection = setting.ComponentDetection;
        DetectionModelPath = detection.ModelPath;
        DetectionInputWidth = detection.InputWidth;
        DetectionInputHeight = detection.InputHeight;
        DetectionConfThreshold = detection.ConfThreshold;
        DetectionIouThreshold = detection.IouThreshold;

        ClassNames.Clear();
        var names = detection.ClassNames;
        for (var i = 0; i < names.Count; i++)
        {
            ClassNames.Add(new ClassNameItem
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

    public void Reset()
    {
        Load();
        SaveStatusText = "Đã đặt lại cài đặt đã lưu gần nhất từ Config/setting.json.";
    }

    public void AddClassName()
    {
        ClassNames.Add(new ClassNameItem
        {
            Index = ClassNames.Count + 1,
            Name = string.Empty
        });
    }

    public void RemoveClassName(ClassNameItem item)
    {
        if (!ClassNames.Remove(item)) return;
        ReindexClassNames();
    }

    public void Save()
    {
        ReindexClassNames();

        var trimmedNames = ClassNames
            .Select(r => r.Name.Trim())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        var setting = _appSettingService.Load();
        setting.DeveloperMode = DeveloperMode;
        setting.ShowInspectionResultAfterRecognition = ShowInspectionResultAfterRecognition;
        setting.Com = Com.Trim();
        setting.WarehouseCom = WarehouseCom.Trim();
        setting.BaudRate = BaudRate;
        setting.StepsPerDeg = StepsPerDeg;
        setting.JogStepDegrees = JogStepDegrees;
        setting.SpeedPercent = Math.Clamp(SpeedPercent, 0, 100);
        setting.DatabaseConnection = DatabaseConnection.Trim();

        setting.ComponentDetection.ModelPath = DetectionModelPath.Trim();
        setting.ComponentDetection.InputWidth = Math.Max(1, DetectionInputWidth);
        setting.ComponentDetection.InputHeight = Math.Max(1, DetectionInputHeight);
        setting.ComponentDetection.ConfThreshold = DetectionConfThreshold;
        setting.ComponentDetection.IouThreshold = DetectionIouThreshold;
        setting.ComponentDetection.ClassNames = trimmedNames;

        setting.PcbBoard.WidthMm = Math.Max(0, PcbBoardWidthMm);
        setting.PcbBoard.HeightMm = Math.Max(0, PcbBoardHeightMm);
        setting.PcbBoard.QuadRectTolerancePercent = HolderQuadRectTolerancePercent;
        setting.PcbBoard.AspectRatioTolerancePercent = HolderAspectRatioTolerancePercent;
        setting.PcbBoard.QuadAngleToleranceDegrees = HolderQuadAngleToleranceDegrees;

        setting.CameraCapture.SaveFolder = CameraCaptureSaveFolder.Trim();

        _appSettingService.Save(setting);
        _appSettingService.SaveComponentDetection(setting.ComponentDetection);

        SpeedPercent = setting.SpeedPercent;
        PcbBoardWidthMm = setting.PcbBoard.WidthMm;
        PcbBoardHeightMm = setting.PcbBoard.HeightMm;
        HolderQuadRectTolerancePercent = setting.PcbBoard.QuadRectTolerancePercent;
        HolderAspectRatioTolerancePercent = setting.PcbBoard.AspectRatioTolerancePercent;
        HolderQuadAngleToleranceDegrees = setting.PcbBoard.QuadAngleToleranceDegrees;

        ClassNames.Clear();
        for (var i = 0; i < trimmedNames.Count; i++)
        {
            ClassNames.Add(new ClassNameItem
            {
                Index = i + 1,
                Name = trimmedNames[i]
            });
        }

        SaveStatusText = "Đã lưu vào Config/setting.json.";
    }

    private void ReindexClassNames()
    {
        for (var i = 0; i < ClassNames.Count; i++)
            ClassNames[i].Index = i + 1;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
