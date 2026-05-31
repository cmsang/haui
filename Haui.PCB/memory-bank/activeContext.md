# Active Context — Haui.PCB

## Current focus

**Fiducial hole detection** — sau Morphology Close, pipeline ưu tiên template matching 4 lỗ tròn định vị (nếu đã có mẫu trong thư mục cấu hình); fallback contour như cũ.

## Recent change

- `FiducialHoleTemplateService` — thư viện `hole_*.png` (nhiều mẫu, lỗ giống nhau); settings `fiducial_settings.json`
- `FiducialHoleDetectionService` — so khớp **tất cả** mẫu trên ảnh Close, lấy 4 vị trí cao nhất (không gán mẫu theo góc)
- MainWindow: chọn thư mục, **Thêm mẫu lỗ**, `FiducialTemplateWindow` (một vùng/lần lưu)
- `PipelineDebugService` — bước debug **Fiducial Matching**

## Open decisions

1. **Unify template storage** — Should `TestPipelineWindow` use `TemplateLibraryService`?
2. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench
3. **Ngưỡng matching** — `MinMatchScore` trong `fiducial_settings.json` (mặc định 0.55); chưa có slider UI

## Files to read first for common tasks

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/BaslerCameraService.cs` |
| Fiducial holes | `Processing/FiducialHoleDetectionService.cs`, `Views/FiducialTemplateWindow.xaml.cs` |
| Segmentation pipeline | `Processing/PcbSegmentationService.cs` |
| Basler parameters | `Processing/ICameraParameterService.cs`, `camera_basler_defaults.json` |
