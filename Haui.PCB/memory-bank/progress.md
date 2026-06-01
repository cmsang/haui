# Progress — Haui.PCB

## Working features

- [x] Basler GigE/USB camera via pylon SDK (`BaslerCameraService`) — **only** camera backend
- [x] **Basler parameter UI** on MainWindow (Exposure, Gain, Gamma; Apply / Reset from `camera_basler_defaults.json`)
- [x] Optional ROI selection on main preview (`last_region.json`)
- [x] PCB segmentation (rotated rect → perspective-corrected board image)
- [x] **Canny thresholds** on MainWindow (slider 0–255; shared by Test / Test 2 / Tạo mẫu)
- [x] **4 lỗ tròn định vị** — thư viện `hole_*.png`; matching downscale + cache RAM; fast path Segment
- [x] Pipeline step debugger window (Test 2)
- [x] Create template: draw regions on segmented board, save PNG + JSON; bắt buộc đủ N vùng (`RequiredRegionCount`, mặc định 18) trước khi lưu
- [x] Multi-template: mỗi ảnh mẫu vùng linh kiện độc lập; thư viện lưu khi tất cả mẫu đủ N vùng
- [x] Create template: tùy chọn thư mục lưu tùy chỉnh (thư viện + mẫu active)
- [x] Multi-template library (`templates/index.json`) with viewer and edit/delete
- [x] Library without index.json — scan `*.png` + `*_regions.json` (`TemplateRegionsDocument`)
- [x] Test pipeline: segment + compare vs all library templates (chỉ thư viện, không `template_board`)
- [x] Region overlay visualization (match green / mismatch red)
- [x] Sharpest-frame selection in camera service (Laplacian variance)
- [x] Region compare preprocessing: 128×128, LAB-L + CLAHE + bilateral before histogram

## Known limitations

- **Basler requires pylon x64** installed; build uses `PlatformTarget=x64`
- **Parameter sliders** use fixed ranges; real camera min/max not yet bound to UI
- **Paths relative to CWD** — running from different folders breaks saved templates
- **No unit/integration tests** in repository
- **No persistence** of comparison history or export

## Suggested next improvements

1. Centralize data directory (e.g. `Environment.SpecialFolder.ApplicationData`)
2. Add `App.xaml.cs` service registration for shared `ICameraService` lifetime
3. Export comparison results (CSV/JSON) from `TestPipelineViewModel`
4. Configurable match threshold (currently hardcoded 80% in `RegionComparisonResult`)

## Build status

Last documented command: `dotnet build Haui.PCB.slnx` — verify after major changes.
