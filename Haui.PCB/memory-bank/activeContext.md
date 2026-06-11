# Active Context — Haui.PCB

## Current focus

Stable layout: **Models** and **Processing** by domain subfolder; **Views** as `Windows/` · `Tabs/` · `Controls/`; config unified in **`Config/setting.json`**.

## Configuration (single file)

`Config/setting.json` via `AppSettingService`:

| Section | Purpose |
|---------|---------|
| `ComponentTemplates` | Library folder, `MinMatchSimilarityPercent`, `AllowedRegionNames`, `RequiredRegionCount` |
| `FiducialHoles` | Fiducial template folder, `MinMatchScore`, `MaxMatchDimension` |
| `CameraBasler` / `CameraCapture` | GenICam defaults, quick-capture save folder |
| Robot | `com`, `DatabaseConnection`, … |

Legacy files (`appsettings.json`, `component_template_settings.json`, `fiducial_settings.json`) auto-migrate on load.

## Open decisions

1. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench
2. **Fiducial UI** — `MinMatchScore` / `MaxMatchDimension` in setting.json; no slider UI yet
3. **Job History / Setting tabs** — placeholder content on MainWindow sidebar

## Entry files by task

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/Camera/BaslerCameraService.cs` |
| Fiducial holes | `Processing/Fiducial/FiducialHoleDetectionService.cs`, `Views/Windows/FiducialTemplateWindow.xaml.cs` |
| Segmentation | `Processing/Segmentation/PcbSegmentationService.cs` |
| Basler parameters | `Processing/Camera/ICameraParameterService.cs`, `Config/setting.json` → `CameraBasler` |
| App config | `Processing/Configuration/AppSettingService.cs`, `Models/Configuration/AppSetting.cs` |
| Template library / test | `Processing/Templates/TemplateLibraryService.cs`, `ViewModels/TestPipelineViewModel.cs` |
