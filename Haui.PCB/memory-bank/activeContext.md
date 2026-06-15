# Active Context — Haui.PCB

## Current focus

Stable layout: **Models** and **Processing** by domain subfolder; **Views** as `Windows/` · `Tabs/` · `Controls/`; config unified in **`Config/setting.json`**.

Language convention (`.cursor/rules/language-and-ui-text.mdc`): source comments **English**; operator UI **Vietnamese** with `MaterialDesignFont` / `Segoe UI` (not Consolas for labels). Mojibake in 13 ViewModel/code-behind `.cs` files fixed (2026-06); child windows/tabs now set `MaterialDesignFont` on root.

**Main tab layout (2026-06-14):** `ContentControl` stretch + tab `RowDefinition` Auto/`*` — fixes broken UI after tab refactor.

**Dashboard inline inspection (2026-06-14):** Wireframe layout — camera (left), result + PASS/FAIL (right), pipeline steps gallery (bottom). **Test** chụp ảnh và kiểm tra tại chỗ trên tab; PASS khi `IsFullMatch`. Đã xóa popup không dùng: `TestPipelineWindow`, `RobotTeachingWindow`, `ManualControlWindow` (thay bằng tab).

**Shell Monitor panel (2026-06-14):** Load/Unload + AGV gauges extracted to `Views/Controls/MonitorView` — sticky above `MainContentHost`; state in `MonitorViewModel` owned by `MainWindow.Monitor`. Tabs receive via `LineMonitor` / `Initialize(..., lineMonitor)`.

**Segmentation fiducial-only (2026-06-14):** Removed contour / MinAreaRect fallback from `PcbSegmentationService`; warp requires 4 matched fiducial holes. Debug pipeline drops Contour Detection and Bounding Quad steps.

**Memory bank (2026-06-15):** Restored from `cmsang/pcb` + added **`memory-bank/baslerCamera.md`** (Haui.PCB implementation + official/community pylon samples).

**Pipeline step timing (2026-06-15):** Dashboard gallery + `PipelineStepsWindow` show per-step elapsed ms (`Stopwatch` in `PcbSegmentationService` → `SegmentationPipelineResult.StepTimings`); description moved to tooltip.

**UI thread / preview perf (2026-06-15):** Camera preview resize + `ToBitmapSource` moved off pylon grab thread with frame drop; Dashboard coalesces `CameraImage` updates (`DispatcherPriority.Render`); Test runs inspection then pipeline steps sequentially; `TestPipelineViewModel` bitmap conversion on thread pool.

**GigE connect speed (2026-06-15):** `CameraBasler.deviceIp` → announce + `ICameraInfo` connect (not `new Camera(ip)`); GigE-only enumerate; resolution probe cached per serial; `StartAsync` off UI thread. Fix: open by serial/`ICameraInfo`, restore `WidthMax`/`HeightMax` probe.

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
3. **Job History tab** — placeholder; **Setting tab** edits `Config/setting.json` (DeveloperMode, robot, DB, ComponentTemplates incl. AllowedRegionNames grid, Fiducial, CameraCapture)

## Recent UI (2026-06-13)

- Removed Dashboard sidebar: Basler exposure/gain/gamma + Canny threshold sliders
- Removed Camera Basler section from Setting tab (runtime defaults still in `setting.json` → `CameraBasler`)
- **Thêm mẫu lỗ** toolbar button between **Tạo mẫu** and **Xem mẫu** — visible only when DeveloperMode on

## Entry files by task

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/Camera/BaslerCameraService.cs` |
| Basler connection & samples | `memory-bank/baslerCamera.md` |
| Fiducial holes | `Processing/Fiducial/FiducialHoleDetectionService.cs`, `Views/Windows/FiducialTemplateWindow.xaml.cs` |
| Segmentation | `Processing/Segmentation/PcbSegmentationService.cs` |
| Basler defaults (file only) | `Config/setting.json` → `CameraBasler`, `Processing/Camera/BaslerCameraService.cs` |
| App config | `Processing/Configuration/AppSettingService.cs`, `ViewModels/SettingViewModel.cs`, `Views/Tabs/SettingTabView.xaml` |
| Template library / test | `Processing/Templates/TemplateLibraryService.cs`, `ViewModels/TestPipelineViewModel.cs` |
