# Progress — Haui.PCB

## MainWindow (2026-06)

- [x] Vietnamese UI strings — mojibake fixed in ViewModels/windows code-behind; `MaterialDesignFont` on all operator windows/tabs
- [x] Sidebar → `ContentControl` with tab views
- [x] UI layers: `Views/Windows/`, `Views/Tabs/`, `Views/Controls/`
- [x] Models / Processing domain subfolders + `GlobalUsings.cs`
- [x] Setting tab — full `setting.json` editor; AllowedRegionNames DataGrid (STT + tên, thêm/sửa/xóa); DeveloperMode + Virtual Serial
- [ ] Job History — detailed content (placeholder)

## Working features

- [x] Basler camera via pylon (`BaslerCameraService`, x64); defaults from `setting.json` → `CameraBasler`
- [x] Quick capture → PNG in `CameraCapture.SaveFolder`
- [x] Optional ROI on preview (`last_region.json`)
- [x] PCB segmentation (rotated rect → perspective warp)
- [x] Fiducial holes — `hole_*.png` library; downscale + RAM cache; fast Segment path
- [x] Pipeline step debugger (`PipelineStepsWindow`)
- [x] Create template — draw regions, save PNG + `*_regions.json`; names ∈ `AllowedRegionNames`
- [x] Template library — scan `*.png` + `*_regions.json` (no `index.json`); folder via `ComponentTemplates.CustomFolder`
- [x] Test pipeline — composite match by `AllowedRegionNames`; 180° retry; overlay green/red
- [x] Region compare — 128×128, LAB-L + CLAHE + bilateral, threshold from `MinMatchSimilarityPercent`
- [x] Unified config — `Config/setting.json` (`AppSettingService`)
- [x] DeveloperMode — hides Tạo mẫu / Thêm mẫu lỗ on Dashboard toolbar; Sửa in template viewer
- [x] Virtual Serial Port — `VirtualRobotSerialService` when dev + virtual enabled (no COM required)
- [x] Sharpest-frame selection (Laplacian variance)

## Known limitations

- Basler requires pylon x64; `PlatformTarget=x64`
- Parameter sliders use fixed ranges (not camera min/max) — removed from Dashboard; edit `CameraBasler` in `setting.json` if needed
- Data paths relative to process CWD — different launch folder breaks saved files
- No unit/integration tests
- No comparison history export

## Suggested next

1. Centralize data directory (e.g. `Environment.SpecialFolder.ApplicationData`)
2. Shared `ICameraService` lifetime via `App.xaml.cs`
3. Export comparison results (CSV/JSON) from `TestPipelineViewModel`
4. Fiducial threshold sliders in UI

## Build status

`dotnet build Haui.PCB.slnx` — **OK** (2026-06-12). NU1701 on `Expression.Blend.Sdk.WPF`.
