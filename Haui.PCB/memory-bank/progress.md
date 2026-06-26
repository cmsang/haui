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
- [x] Holder quad validation — `QuadGeometry` + `PcbBoard` aspect ratio; `quadRectTolerancePercent` / `aspectRatioTolerancePercent` / `quadAngleToleranceDegrees` (default 15%/15%/5°); Setting tab **Hộp đỡ PCB**
- [x] Pipeline step gallery on Dashboard — per-step elapsed time via `PipelineStepMapper`; bước **Khung hộp đỡ** annotate tứ giác trên ảnh gốc; bước **Nhận diện linh kiện** sau segmentation
- [x] Create template — draw regions, save PNG + `*_regions.json`; names ∈ `AllowedRegionNames`, unique per template (validate before save); một CheckBox **Đặt vị trí thành phần xác định chiều** (kéo trên ảnh, vùng cyan nét đứt, không nằm trong danh sách linh kiện); zoom ảnh Ctrl+cuộn / −+ / **Vừa khung** (25%–800%) để đánh vùng chính xác
- [x] Board orientation — config `orientationComponentName` + `minOrientationMatchScore`; auto 180° retry; restrict component match to matched template; gallery step **Xác định chiều mạch**; **`OrientationMarkerCache`** grayscale crop cache (2026-06-22)
- [x] Template library — scan `*.png` + `*_regions.json` (no `index.json`); folder via `ComponentTemplates.CustomFolder` or **`whiteCircuitCustomFolder`** when `trainWhiteCircuit`
- [x] **White-circuit inspection** — per-region compare against white-circuit library; template match = absent component; PASS when all regions have components; Setting **Train mạch trắng**; separate library folder
- [x] Test pipeline — composite match by `AllowedRegionNames`; overlay green/red; every configured name reported (`RegionMatchOutcome`)
- [x] Dashboard inline test — result image + panel kết quả dạng bảng (Kết quả x/y + PASS/FAIL + tổng thời gian pipeline, D/s linh kiện thiếu, DataGrid thiếu), pipeline steps gallery on tab (no popup on Test)
- [x] Result overlay — viền đánh dấu vùng linh kiện trên ảnh kết quả tăng x3 độ dày (2 → 6) để dễ nhìn khi vận hành
- [x] Preview perf — off-thread Mat→BitmapSource, frame drop, coalesced Dispatcher updates (2026-06-15)
- [x] Unified Test — single `RunPipeline` feeds PASS/FAIL + step gallery via `PipelineStepMapper` (2026-06-15)
- [x] Region compare — 128×128, LAB-L + CLAHE + bilateral, **hybrid score** (0.7 NCC + 0.3 histogram), threshold from `MinMatchSimilarityPercent`
- [x] Unified config — `Config/setting.json` (`AppSettingService`)
- [x] DeveloperMode — **Chọn ảnh** (file inspect, no camera required), Tạo mẫu / Xem mẫu on Dashboard toolbar; Sửa in template viewer (lưu trực tiếp trong dialog, xóa theo từng dòng)
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
