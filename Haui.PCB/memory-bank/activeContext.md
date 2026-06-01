# Active Context — Haui.PCB

## Current focus

**Thư viện mẫu** — đã bỏ `template_board` / `TemplateRegionService`; Test + Create + Viewer chỉ dùng thư mục `*.png` + `*_regions.json`.

## Recent change

- **Thư viện mẫu:** bỏ `UseCustomFolder` và UI chọn thư mục trên Create; luôn dùng `ComponentTemplates.CustomFolder` trong `appsettings.json` (rỗng → `templates/`)
- **Cấu hình:** gộp `appsettings.json` (ComponentTemplates, FiducialHoles, CameraBasler); migrate tự động từ 3 file cũ nếu còn
- **Mẫu lỗ định vị:** thư mục qua `appsettings.json` → FiducialHoles; đã bỏ chọn thư mục trên MainWindow
- **Test pipeline:** ngưỡng so khớp `MinMatchSimilarityPercent` trong `component_template_settings.json` (mặc định 80); so khớp tổng hợp theo `AllowedRegionNames`
- **Tạo mẫu:** nút «Xoay 180°» cạnh Lưu/Đóng — xoay ảnh bo mạch và cập nhật tọa độ vùng tương đối
- **Test pipeline:** nếu chưa đạt đủ vùng → xoay bo mạch 180° → so lại; hiển thị ảnh xoay + ghi chú trạng thái
- **Tạo mẫu:** lưu khi mọi tên vùng thuộc `AllowedRegionNames` trong `component_template_settings.json` (không bắt đủ 18 vùng); Viewer thư viện vẫn dùng `RequiredRegionCount`
- **Ảnh mẫu linh kiện:** mỗi ảnh trong thư viện có danh sách vùng riêng; tạo mẫu mới không copy vùng từ mẫu active
- **Tạo mẫu linh kiện:** chỉ thư viện PNG + `*_regions.json` (`component_template_settings.json`); Test/Viewer/Create dùng chung
- **Tối ưu Segment (Test / Tạo mẫu):** `RunPipelineCore(includeDebugMats)` — fast path không clone 4 Mat trung gian, bỏ contour khi fiducial OK
- **Fiducial matching:** cache RAM (`FiducialHoleServices.TemplateService`), downscale `MaxMatchDimension=1280`, in-place NMS trên bản đồ match
- `CreateTemplateViewModel.LoadFrameAsync` — Segment chạy ngoài UI thread
- Thư viện `hole_*.png`; so khớp tất cả mẫu; `FiducialTemplateWindow` chọn nhiều vùng, lưu một lần

## Open decisions

1. **Unify template storage** — Should `TestPipelineWindow` use `TemplateLibraryService`?
2. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench
3. **Ngưỡng matching** — `MinMatchScore` / `MaxMatchDimension` trong `fiducial_settings.json`; chưa có slider UI

## Files to read first for common tasks

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/BaslerCameraService.cs` |
| Fiducial holes | `Processing/FiducialHoleDetectionService.cs`, `Views/FiducialTemplateWindow.xaml.cs` |
| Segmentation pipeline | `Processing/PcbSegmentationService.cs` |
| Basler parameters | `Processing/ICameraParameterService.cs`, `camera_basler_defaults.json` |
