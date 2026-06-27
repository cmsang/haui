# System Patterns — Haui.PCB

## Folder structure

```
Haui.PCB/                 # WPF app project
├── GlobalUsings.cs       # global using cho Models.* và Processing.*
├── App.xaml(.cs)         # Startup → Views/Windows/MainWindow
├── Models/               # DTO / POCO theo domain
│   ├── Configuration/    # AppSetting, ComponentDetectionSettings, …
│   ├── Detection/        # MissingComponent, ComponentInspectionResult
│   ├── Segmentation/     # SegmentationPipelineResult, …
│   ├── Camera/           # CameraInfo, ResolutionInfo
│   └── Robot/            # RobotTeachPoint, RobotJointLimits, …
├── Processing/           # Services + I* theo domain
│   ├── Configuration/    # AppSettingService, AppConfigPaths, …
│   ├── Camera/           # BaslerCameraService, CameraCaptureService, …
│   ├── Segmentation/     # PcbSegmentationService, HolderContourDetectionService, QuadGeometry, PipelineStepMapper
│   ├── Detection/        # OnnxYoloDetector, MissingComponentDetectionService
│   └── Robot/            # RobotSerialService, RobotConfigService, …
├── ViewModels/           # INotifyPropertyChanged, business logic
│   └── Pipeline/         # PipelineStep (WPF BitmapSource)
└── Views/                # UI theo layer ngang
    ├── Windows/          # *Window (MainWindow, inspection, robot popup)
    ├── Tabs/             # *TabView (sidebar content)
    └── Controls/         # ArcGauge, …
```

## Architecture

- **MVVM-lite**: ViewModels hold state and commands; Views subscribe to events / `PropertyChanged`
- **DIP without DI container**: depend on `I*` interfaces; concrete services constructed with `new` in window constructors
- **SOLID** enforced by convention — see `.github/copilot-instructions.md`

## Service interfaces (`Processing/`)

| Interface | Implementation | Role |
|-----------|----------------|------|
| `ICameraService` | `BaslerCameraService` | Basler pylon; frames as `Mat` |
| `ICameraParameterService` | `BaslerCameraService` | GenICam Apply / Reset (Basler only) |
| `IPcbSegmentationService` | `PcbSegmentationService` | Canny + holder contour + perspective warp → straight board |
| `IComponentInspectionService` | `MissingComponentDetectionService` | YOLO26 ONNX — detect missing component locations on warped board |
| `IOnnxYoloDetector` | `OnnxYoloDetector` | Letterbox + OnnxRuntime inference + NMS |

Step gallery on Dashboard uses `PipelineStepMapper` + `TestPipelineViewModel.Steps` (segmentation + YOLO recognition).

## ONNX model

Path → `Config/setting.json` → `ComponentDetection.modelPath` (default `Models/yolo26m_960x1280.onnx`). Train/export via `yolo/` project.

## Window graph

```
App → MainWindow (MainViewModel + CameraService)
        └─ DashboardTabView (inline Test + pipeline gallery)
```

Child windows receive a captured `Mat` from Main; caller **disposes** the frame after handoff.

## Threading / WPF

- Camera callbacks are not on UI thread — use `Dispatcher.InvokeAsync` in code-behind for bindings
- Convert `Mat` → `BitmapSource` via `OpenCvSharp.WpfExtensions.BitmapSourceConverter`, then **`.Freeze()`** before binding

## OpenCV resource pattern

```csharp
using var gray = new Mat();
// … always dispose Mats cloned for pipeline branches
```

Never hold long-lived `Mat` on ViewModel without clear ownership; prefer `BitmapSource` for UI.

## Extension pattern

1. Add `IMyService` + implementation in `Processing/{Domain}/`
2. Instantiate in the window constructor (`MainWindow.xaml.cs` as reference)
3. Inject into ViewModel via constructor
