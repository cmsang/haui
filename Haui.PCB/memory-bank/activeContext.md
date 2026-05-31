# Active Context — Haui.PCB

## Current focus

**Fiducial hole detection** — sau Morphology Close, pipeline ưu tiên template matching 4 lỗ tròn định vị (nếu đã có mẫu trong thư mục cấu hình); fallback contour như cũ.

## Recent change

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
