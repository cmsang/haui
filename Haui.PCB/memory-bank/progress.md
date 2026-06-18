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
- [x] PCB segmentation (fiducial holes → perspective warp; contour fallback removed 2026-06-14)
- [x] Fiducial holes — `hole_*.png` library; downscale + RAM cache; fast Segment path
- [x] Fiducial recognition scoring — `hole_recognition_stats.json` per template (+1/hole on success only, mod 100M); ordered template matching
- [x] Fiducial geometric quad — `FiducialQuadSelector` combinatorial search + `PcbBoard` aspect ratio (default 400×550 mm); fallback largest area when mm disabled
- [x] Pipeline step gallery on Dashboard — per-step elapsed time via `PipelineStepMapper` (removed separate `PipelineStepsWindow` 2026-06-16); bước **Lỗ định vị** annotate tâm lỗ + % khớp trên ảnh gốc
- [x] Create template — draw regions, save PNG + `*_regions.json`; names ∈ `AllowedRegionNames`, unique per template (validate before save)
- [x] Template library — scan `*.png` + `*_regions.json` (no `index.json`); folder via `ComponentTemplates.CustomFolder`
- [x] Test pipeline — composite match by `AllowedRegionNames`; overlay green/red; every configured name reported (`RegionMatchOutcome`)
- [x] Dashboard inline test — result image + panel kết quả dạng bảng (Kết quả x/y + PASS/FAIL, D/s linh kiện thiếu, DataGrid thiếu), pipeline steps gallery on tab (no popup on Test)
- [x] Result overlay — viền đánh dấu vùng linh kiện trên ảnh kết quả tăng x3 độ dày (2 → 6) để dễ nhìn khi vận hành
- [x] Preview perf — off-thread Mat→BitmapSource, frame drop, coalesced Dispatcher updates (2026-06-15)
- [x] Unified Test — single `RunPipeline` feeds PASS/FAIL + step gallery via `PipelineStepMapper` (2026-06-15)
- [x] Region compare — 128×128, LAB-L + CLAHE + bilateral, **hybrid score** (0.7 NCC + 0.3 histogram), threshold from `MinMatchSimilarityPercent`
- [x] Unified config — `Config/setting.json` (`AppSettingService`)
- [x] DeveloperMode — **Chọn ảnh** (file inspect, no camera required), **Chọn ảnh lỗ** (file → fiducial template window), Tạo mẫu / Thêm mẫu lỗ on Dashboard toolbar; Sửa in template viewer (lưu trực tiếp trong dialog, xóa theo từng dòng)
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

`dotnet build Haui.PCB.slnx` — **OK** (2026-06-18). NU1701 on `Expression.Blend.Sdk.WPF`.
