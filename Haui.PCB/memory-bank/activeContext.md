# Active Context — Haui.PCB

## Current focus

Stable layout: **Models** and **Processing** by domain subfolder; **Views** as `Windows/` · `Tabs/` · `Controls/`; config unified in **`Config/setting.json`**.

Language convention (`.cursor/rules/language-and-ui-text.mdc`): source comments **English**; operator UI **Vietnamese** with `MaterialDesignFont` / `Segoe UI` (not Consolas for labels). Mojibake in 13 ViewModel/code-behind `.cs` files fixed (2026-06); child windows/tabs now set `MaterialDesignFont` on root.

## Configuration (single file)

`Config/setting.json` via `AppSettingService`:

| Section | Purpose |
|---------|---------|
| `ComponentTemplates` | Library folder, `MinMatchSimilarityPercent`, `AllowedRegionNames` (region count = list size) |
| `FiducialHoles` | Fiducial template folder, `MinMatchScore`, `MaxMatchDimension` |
| `CameraBasler` / `CameraCapture` | GenICam defaults, quick-capture save folder |
| Robot | `com`, `warehouseCom`, `DatabaseConnection`, … |
| Developer | `developerMode`, `virtualSerialPort` (serial simulation; requires dev mode) |

Legacy files (`appsettings.json`, `component_template_settings.json`, `fiducial_settings.json`) auto-migrate on load.

## Open decisions

1. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench
2. **Fiducial UI** — `MinMatchScore` / `MaxMatchDimension` in setting.json; no slider UI yet
3. **Job History tab** — placeholder; **Setting tab** edits full `Config/setting.json` (DeveloperMode, robot, DB, ComponentTemplates incl. AllowedRegionNames grid, Fiducial, Camera)

## Entry files by task

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/Camera/BaslerCameraService.cs` |
| Fiducial holes | `Processing/Fiducial/FiducialHoleDetectionService.cs`, `Views/Windows/FiducialTemplateWindow.xaml.cs` |
| Segmentation | `Processing/Segmentation/PcbSegmentationService.cs` |
| Basler parameters | `Processing/Camera/ICameraParameterService.cs`, `Config/setting.json` → `CameraBasler` |
| App config | `Processing/Configuration/AppSettingService.cs`, `ViewModels/SettingViewModel.cs`, `Views/Tabs/SettingTabView.xaml` |
| Template library / test | `Processing/Templates/TemplateLibraryService.cs`, `ViewModels/TestPipelineViewModel.cs` |
