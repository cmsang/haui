# Progress — Haui.PCB

## MainWindow (2026-06)

- [x] Vietnamese UI strings — mojibake fixed in ViewModels/windows code-behind; `MaterialDesignFont` on all operator windows/tabs
- [x] Sidebar → `ContentControl` with tab views
- [x] UI layers: `Views/Windows/`, `Views/Tabs/`, `Views/Controls/`
- [x] Shell **Monitor** — sticky line-status panel (`MonitorView` + `MonitorViewModel`); Pass→Load++, Fail→Unload++
- [x] Models / Processing domain subfolders + `GlobalUsings.cs`
- [x] Setting tab — full `setting.json` editor; AllowedRegionNames DataGrid (STT + tên, thêm/sửa/xóa); DeveloperMode + Virtual Serial
- [ ] Job History — detailed content (placeholder)

## Working features

- [x] GigE fast connect: `deviceIp` in `setting.json`, direct IP, no probe Open, async Start, deferred AWB
- [x] Basler camera reference doc — `memory-bank/baslerCamera.md` (project + official/community samples)
- [x] Quick capture → PNG in `CameraCapture.SaveFolder`
- [x] Optional ROI on preview (`last_region.json`)
- [x] PCB segmentation (holder support contour → perspective warp; default frame 460×590 mm; gallery **Khung hộp đỡ**)
- [x] **HD holder detection (2026-06-27)** — Canny/Close/hull on downscaled copy (`HolderDetectionDownscale`, default 1920×1080); corners mapped to full-res for warp + YOLO; hull-only (no FindContours fallback)
- [x] **Canny threshold config (2026-06-27)** — `Segmentation` section in `setting.json`; runtime load via `AppSettingsStore`; DeveloperMode Dashboard button + `CannyThresholdWindow` tuner (save to JSON)
- [x] Holder quad validation — `QuadGeometry` + `PcbBoard` aspect ratio; `quadRectTolerancePercent` / `aspectRatioTolerancePercent` / `quadAngleToleranceDegrees` (default 15%/15%/5°); Setting tab **Hộp đỡ PCB**
- [x] Pipeline step gallery on Dashboard — per-step elapsed time via `PipelineStepMapper`; bước **Khung hộp đỡ** annotate tứ giác trên ảnh gốc; bước **Nhận diện linh kiện** sau segmentation
- [x] **YOLO missing-component detection (2026-06-26)** — `OnnxYoloDetector` + `MissingComponentDetectionService` (Microsoft.ML.OnnxRuntime); PASS when zero boxes; red overlay per missing location; config `ComponentDetection` in `setting.json`; removed template library / orientation / histogram match / Create+Viewer windows
- [x] **Component groups + split marking boxes (2026-06-27)** — `componentGroups` + `defaultGroupSplit` in `ComponentDetection`; parent detections expanded to child rows/boxes (horizontal columns default; D12 vertical rows)
- [x] Dashboard inline test — result image + panel (PASS/FAIL, thời gian, DataGrid linh kiện thiếu), pipeline steps gallery on tab
- [x] Result overlay — red boxes at missing component locations detected by YOLO
- [x] Preview perf — off-thread Mat→BitmapSource, frame drop, coalesced Dispatcher updates (2026-06-15)
- [x] Unified Test — single `RunPipeline` feeds PASS/FAIL + step gallery via `PipelineStepMapper` (2026-06-15)
- [x] Region compare — removed; replaced by YOLO ONNX (2026-06-26)
- [x] Unified config — `Config/setting.json` (`AppSettingService`)
- [x] DeveloperMode — **Chọn ảnh** (file inspect, no camera required) on Dashboard toolbar
- [x] Virtual Serial Port — `VirtualRobotSerialService` when dev + virtual enabled (no COM required)
- [x] Sharpest-frame selection (Laplacian variance)
- [x] Camera Mono8 — force `Mono8`, `CV_8UC1` grab, Gaussian blur at grab; segmentation skips Grayscale + duplicate blur for 1-channel frames

## Known limitations

- Basler requires pylon x64; `PlatformTarget=x64`
- Parameter sliders use fixed ranges (not camera min/max) — removed from Dashboard; edit `CameraBasler` in `setting.json` if needed
- Data paths relative to process CWD — different launch folder breaks saved files
- **Templates created before holder-contour segmentation** must be re-created on the holder frame crop
- No comparison history export

## Suggested next

1. Centralize data directory (e.g. `Environment.SpecialFolder.ApplicationData`)
2. Shared `ICameraService` lifetime via `App.xaml.cs`
3. Export comparison results (CSV/JSON) from `TestPipelineViewModel`

## Build status

`dotnet build Haui.PCB.slnx` — **OK** (2026-06-22, orientation marker cache). NU1701 on `Expression.Blend.Sdk.WPF`.
