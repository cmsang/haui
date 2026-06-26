# Active Context — Haui.PCB

## Current focus

Stable layout: **Models** and **Processing** by domain subfolder; **Views** as `Windows/` · `Tabs/` · `Controls/`; config unified in **`Config/setting.json`**.

Language convention (`.cursor/rules/language-and-ui-text.mdc`): source comments **English**; operator UI **Vietnamese** with `MaterialDesignFont` / `Segoe UI` (not Consolas for labels). Mojibake in 13 ViewModel/code-behind `.cs` files fixed (2026-06); child windows/tabs now set `MaterialDesignFont` on root.

**Main tab layout (2026-06-14):** `ContentControl` stretch + tab `RowDefinition` Auto/`*` — fixes broken UI after tab refactor.

**Dashboard inline inspection (2026-06-14):** Wireframe layout — camera (left), result + PASS/FAIL (right), pipeline steps gallery (bottom). **Test** chụp ảnh và kiểm tra tại chỗ trên tab; PASS khi `IsFullMatch`. Đã xóa popup không dùng: `TestPipelineWindow`, `RobotTeachingWindow`, `ManualControlWindow` (thay bằng tab).

**Shell Monitor panel (2026-06-14):** Load/Unload + AGV gauges extracted to `Views/Controls/MonitorView` — sticky above `MainContentHost`; state in `MonitorViewModel` owned by `MainWindow.Monitor`. Tabs receive via `LineMonitor` / `Initialize(..., lineMonitor)`.

**Segmentation fiducial-only (2026-06-14):** Removed contour / MinAreaRect fallback from `PcbSegmentationService`; warp requires 4 matched fiducial holes. Debug pipeline drops Contour Detection and Bounding Quad steps.

**Memory bank (2026-06-15):** Restored from `cmsang/pcb` + added **`memory-bank/baslerCamera.md`** (Haui.PCB implementation + official/community pylon samples).

**Template data (2026-06-15):** `pcb_templates/` (11 mẫu) và `fiducial_holes/` (128 ảnh) checkout từ `cmsang/pcb` → nhánh `cmsang/pcb-new` (staged, chưa commit).

**Pipeline step timing (2026-06-15):** Dashboard gallery shows per-step elapsed ms (`Stopwatch` in `PcbSegmentationService` → `SegmentationPipelineResult.StepTimings`); description moved to tooltip. Dashboard gallery row height **248px** (was 220) so elapsed-time footer is not clipped when horizontal scrollbar shows.

**Component recognition timing (2026-06-21):** `TestPipelineViewModel` — gallery bước **Nhận diện linh kiện** (ảnh annotate trên bo mạch đã cắt, footer = thời gian `CompositeTemplateMatchService.Match`); PASS/FAIL panel hiển thị `TotalInspectionElapsedText` (tổng segmentation + nhận diện linh kiện).

**UI thread / preview perf (2026-06-15):** Camera preview resize + `ToBitmapSource` moved off pylon grab thread with frame drop; Dashboard coalesces `CameraImage` updates (`DispatcherPriority.Render`); Test runs inspection then pipeline steps sequentially; `TestPipelineViewModel` bitmap conversion on thread pool.

**GigE connect speed (2026-06-15):** `CameraBasler.deviceIp` → announce + `ICameraInfo` connect (not `new Camera(ip)`); GigE-only enumerate; resolution probe cached per serial; `StartAsync` off UI thread. Fix: open by serial/`ICameraInfo`, restore `WidthMax`/`HeightMax` probe.

**Basler connect defaults (2026-06-20):** `CameraBasler` trong `setting.json` — `pixelFormat` (mặc định `Mono8`, fallback `BGR8` → Bayer*), `gainAuto` và `balanceWhiteAuto` (`Continuous`); `BaslerCameraService` đọc từ config khi kết nối; bỏ ghi `Gain` thủ công khi `gainAuto` ≠ `Off`.

**Camera Mono8 pipeline (2026-06-21):** `BaslerCameraService` ép `Mono8` only → `Mat` `CV_8UC1`; Gaussian blur 5×5 ngay khi grab; segmentation bỏ bước Grayscale và **không blur lại** ảnh 1 kênh (gallery không có bước Gaussian Blur trùng); file màu vẫn BGR→gray + blur trong pipeline.

**Edge AABB fiducial ROI (2026-06-20):** Sau Morphology Close, `PcbSegmentationService` tính ROI = `BoundingRect(FindNonZero)` trên pixel biên; `FiducialHoleDetectionService.Detect` chạy trên crop ROI; tọa độ lỗ cộng offset trước warp. `SegmentationPipelineResult.EdgeSearchRoi` + gallery Morphology Close vẽ khung cam.

**Fiducial corner-first search (2026-06-20):** `FiducialSearchZones` — match theo vùng góc/cạnh từ ngoài vào trên ROI; early exit khi đủ quad hợp lệ; không fallback match toàn ROI.

**Fiducial template form ROI (2026-06-20):** `FiducialTemplateWindow` / `LoadFrame` crop Morphology Close → Edge AABB ROI (`EdgeSearchRoiHelper`); operator chọn vùng trên ảnh ROI.

**Dashboard cleanup (2026-06-16):** Removed Test 2 + `PipelineStepsWindow` / `PipelineDebugService`. DeveloperMode **Chọn ảnh** runs `TestPipelineViewModel.InspectFromFileAsync` — results in `ResultImage` + gallery only; `CameraImage` remains live camera feed.

**Fiducial template from file (2026-06-16):** DeveloperMode **Chọn ảnh lỗ** → `MainViewModel.LoadFiducialTemplateFromFileAsync` (ImRead + Morphology Close) → `FiducialTemplateWindow`; no camera required. Camera path **Thêm mẫu lỗ** unchanged (ROI crop + grab).

**Delete icon button (2026-06-16):** Inline row delete actions use shared `Views/Controls/DeleteIconButton` + app-wide `DeleteIconButtonStyle` (`App.xaml`); used in FiducialTemplateWindow, CreateTemplateWindow, SettingTabView AllowedRegionNames grid. TemplateViewerWindow cũng dùng DeleteIconButton cho cột xóa theo từng dòng (bỏ nút Lưu thư viện). Delete buttons right-aligned per row (`StretchListBoxItemStyle` on ListBoxes; DataGrid cell `HorizontalContentAlignment=Right`).

**Dashboard component panel (2026-06-16):** Đã vẽ lại panel kết quả dạng bảng theo mẫu vận hành: dòng **Kết quả** (`x/y` + PASS/FAIL + tổng thời gian pipeline), dòng **D/s linh kiện thiếu**, tiêu đề đỏ gạch chân **Danh sách linh kiện thiếu**, và DataGrid danh sách thiếu ngay bên dưới. Vẫn bind `TestPipelineViewModel` qua `ComponentResultsPanel.DataContext`; `PassFailPanel`/`PassFailText` giữ nguyên để code-behind đổi màu theo PASS/FAIL; thời gian bind `TotalInspectionElapsedText`. **2026-06-21:** PASS/FAIL và tổng thời gian xử lý nằm ngang hàng — hai `Border` riêng (`InspectionTimingPanel` trước, `PassFailPanel` sau) trong `StackPanel` ngang.

**Result overlay border thickness (2026-06-16):** `TestPipelineViewModel.DrawAnnotations` tăng độ dày viền vùng linh kiện trên ảnh kết quả từ `2` lên `6` (x3) để dễ quan sát PASS/FAIL theo từng vùng.

**Hybrid region similarity (2026-06-16):** `RegionComparisonService` đổi metric production từ histogram-only sang **0.7 NCC + 0.3 histogram correlation** sau cùng pipeline tiền xử lý (resize 128×128, LAB-L, CLAHE, bilateral). Mục tiêu: nhạy hơn với thiếu/sai linh kiện nhưng vẫn ổn định khi ánh sáng biến thiên nhẹ.

**Fiducial recognition scoring (2026-06-17):** `hole_recognition_stats.json` — chỉ khi detect thành công 4 lỗ: mỗi mẫu +1 điểm/lỗ nhận diện (tối đa +4/lần), mẫu không khớp giữ nguyên (mặc định 0); điểm lưu `(cũ + mới) % 100_000_000`; `LoadStats` clamp âm về 0; sort DESC; UI `FiducialTemplateWindow` hiển thị `+N`.

**Pipeline fiducial annotation (2026-06-16):** Bước gallery **Lỗ định vị** vẽ annotation trên ảnh gốc màu: vòng tròn + số thứ tự + % khớp; tứ giác cam khi đủ 4 lỗ (xanh), vàng khi thiếu lỗ. Partial detect trả về tâm lỗ để hiển thị debug.

**Fiducial quad ordering fix (2026-06-16):** `FiducialQuadOrdering.OrderCorners` thay `OrderPoints` cũ (IndexOf trùng góc) — warp không còn suy biến; gallery downscale max 1920px + marker lớn hơn để thấy trên thumbnail.

**Fiducial geometric quad selection (2026-06-16):** `FiducialQuadSelector` + `FiducialQuadGeometry` — combinatorial search trong pool ứng viên (dedupe, max 15); lọc tứ giác lồi + rectangularity; ràng buộc tỷ lệ cạnh từ `PcbBoard` (mặc định **400×550 mm**); width/height = 0 → fallback diện tích lớn nhất. `FiducialHoles`: `aspectRatioTolerance`, `maxQuadSearchCandidates`, `minQuadRectangularity`. Setting tab: chiều rộng/cao bo mạch (mm).

**Orientation marker cache (2026-06-22):** `OrientationMarkerCache` — preload grayscale crop vùng xác định chiều (`KF3_LABEL` / `orientationComponentName`) một lần theo thư mục thư viện; `BoardOrientationDetectionService` chỉ crop bo mạch đang kiểm tra + `CompareGrayscale`. Invalidate khi lưu/xóa mẫu (`TemplateLibraryService`) hoặc mtime PNG/JSON đổi. Giảm ~21× `ImRead` PNG đầy đủ mỗi lần Kiểm tra.

**Orientation marker cache (2026-06-22):** `OrientationMarkerCache` — preload grayscale crop vùng xác định chiều (`KF3_LABEL` / `orientationComponentName`) một lần theo thư mục thư viện; `BoardOrientationDetectionService` chỉ crop bo mạch đang kiểm tra + `CompareGrayscale`. Invalidate khi lưu/xóa mẫu (`TemplateLibraryService`) hoặc mtime PNG/JSON đổi. Giảm ~21× `ImRead` PNG đầy đủ mỗi lần Kiểm tra.

**Create template region validation (2026-06-17):** Trước khi lưu mẫu PCB — tên vùng phải ∈ `ComponentTemplates.AllowedRegionNames` và không trùng; `CanSave`, validate trước lưu. UI: ComboBox chọn tên từ danh sách cấu hình (không gõ tự do); vùng mới tự gán tên chưa dùng.

**Create template zoom (2026-06-21):** `CreateTemplateWindow` — ScrollViewer + zoom 25%–800% (Ctrl+cuộn chuột hoặc nút −/+ / **Vừa khung**); zoom qua `LayoutTransform` (canvas giữ hệ tọa độ cố định, ảnh + overlay scale đồng bộ); kéo vùng map `GetPosition(BoardViewHost)` → tọa độ tương đối.

**Template windows fullscreen (2026-06-18):** `CreateTemplateWindow` và `TemplateViewerWindow` mở `WindowState=Maximized` (giống `MainWindow`).

**Inspection rotation removed (2026-06-18):** `TestPipelineViewModel` không còn xoay bo mạch 180° tự động khi so khớp linh kiện; bo mạch phải đúng hướng mẫu mới PASS. Nút **Xoay 180°** trong Tạo mẫu (`CreateTemplateViewModel.RotateBoard180`) vẫn giữ cho chỉnh mẫu thủ công.

**Pipeline fiducial annotation (2026-06-16):** Bước gallery **Lỗ định vị** vẽ annotation trên ảnh gốc màu: vòng tròn + số thứ tự + % khớp; tứ giác cam khi đủ 4 lỗ (xanh), vàng khi thiếu lỗ. Partial detect trả về tâm lỗ để hiển thị debug.

**Fiducial quad ordering fix (2026-06-16):** `FiducialQuadOrdering.OrderCorners` thay `OrderPoints` cũ (IndexOf trùng góc) — warp không còn suy biến; gallery downscale max 1920px + marker lớn hơn để thấy trên thumbnail.

**Fiducial geometric quad selection (2026-06-16):** `FiducialQuadSelector` + `FiducialQuadGeometry` — combinatorial search trong pool ứng viên (dedupe, max 15); lọc tứ giác lồi + rectangularity; ràng buộc tỷ lệ cạnh từ `PcbBoard` (mặc định **400×550 mm**); width/height = 0 → fallback diện tích lớn nhất. `FiducialHoles`: `aspectRatioTolerance`, `maxQuadSearchCandidates`, `minQuadRectangularity`. Setting tab: chiều rộng/cao bo mạch (mm).

**Create template region validation (2026-06-17):** Trước khi lưu mẫu PCB — tên vùng phải ∈ `ComponentTemplates.AllowedRegionNames` và không trùng; `CanSave`, validate trước lưu. UI: ComboBox chọn tên từ danh sách cấu hình (không gõ tự do); vùng mới tự gán tên chưa dùng.

## Configuration (single file)

`Config/setting.json` via `AppSettingService`:

| Section | Purpose |
|---------|---------|
| `ComponentTemplates` | Library folder, `MinMatchSimilarityPercent`, `AllowedRegionNames` (region count = list size) |
| `FiducialHoles` | Fiducial template folder, `MinMatchScore`, `MaxMatchDimension`, quad-search tuning |
| `PcbBoard` | Board `widthMm` / `heightMm` (default 400×550) for fiducial aspect-ratio filter |
| `CameraBasler` / `CameraCapture` | GenICam defaults, quick-capture save folder |
| Robot | `com`, `warehouseCom`, `DatabaseConnection`, … |
| Developer | `developerMode`, `virtualSerialPort` (serial simulation; requires dev mode) |

Legacy files (`appsettings.json`, `component_template_settings.json`, `fiducial_settings.json`) auto-migrate on load.

## Open decisions

1. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench
2. **Fiducial UI** — board mm + `MinMatchScore` / `MaxMatchDimension` in Setting tab; `aspectRatioTolerance` etc. JSON-only for now
3. **Job History tab** — placeholder; **Setting tab** edits `Config/setting.json` (DeveloperMode, robot, DB, ComponentTemplates incl. AllowedRegionNames grid, Fiducial, CameraCapture)

## Recent UI (2026-06-13)

- Removed Dashboard sidebar: Basler exposure/gain/gamma + Canny threshold sliders
- Removed Camera Basler section from Setting tab (runtime defaults still in `setting.json` → `CameraBasler`)
- **Thêm mẫu lỗ** toolbar button between **Tạo mẫu** and **Xem mẫu** — visible only when DeveloperMode on
- **Chọn ảnh lỗ** (DeveloperMode) — file → Morphology Close → `FiducialTemplateWindow`; no camera required

## Entry files by task

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/Camera/BaslerCameraService.cs` |
| Basler connection & samples | `memory-bank/baslerCamera.md` |
| Fiducial holes | `Processing/Fiducial/FiducialHoleDetectionService.cs`, `FiducialQuadSelector.cs`, `Views/Windows/FiducialTemplateWindow.xaml.cs` |
| Segmentation | `Processing/Segmentation/PcbSegmentationService.cs` |
| Basler defaults (file only) | `Config/setting.json` → `CameraBasler`, `Processing/Camera/BaslerCameraService.cs` |
| App config | `Processing/Configuration/AppSettingService.cs`, `ViewModels/SettingViewModel.cs`, `Views/Tabs/SettingTabView.xaml` |
| Template library / test | `Processing/Templates/TemplateLibraryService.cs`, `ViewModels/TestPipelineViewModel.cs` |
