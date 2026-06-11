# Active Context — Haui.PCB

## Current focus

**Models / Processing** — đã tổ chức theo domain subfolder; `GlobalUsings.cs` import namespace con.

## Recent change

- **Tổ chức Models + Processing:** `Models/{Configuration,Templates,Segmentation,Camera,Robot}/`, `Processing/{Configuration,Camera,Segmentation,Templates,Fiducial,Robot}/`; `PipelineStep` → `ViewModels/Pipeline/`; `CameraModels` tách → `Models/Camera/`; `RobotTeachConfig.cs` đổi tên → `RobotJointLimits.cs`
- **UI layer ngang:** `Views/Windows/` (MainWindow + inspection + `RobotTeachingWindow` / `ManualControlWindow`), `Views/Tabs/` (5 tab), `Views/Controls/` (ArcGauge); `ViewModels/` giữ nguyên

- **Build fix:** sửa `HintPath` `Microsoft.Expression.Drawing`; khôi phục `MainWindow.xaml.cs` (gỡ duplicate merge); xóa duplicate `MAIN CONTENT` + `BtnCapture` trong `MainWindow.xaml`; thêm lại `Basler.Pylon` + `PlatformTarget=x64` trong csproj
- **Cấu hình thống nhất:** một file `Config/setting.json` — robot (`com`, `DatabaseConnection`…) + vision (`ComponentTemplates`, `FiducialHoles`, `CameraBasler`, `CameraCapture`); đã xóa `appsettings.json`; migrate tự động từ file cũ nếu còn
- **Thư viện mẫu:** `ComponentTemplates.CustomFolder` trong `setting.json` (rỗng → `templates/`)
- **Chụp ảnh nhanh:** lưu PNG vào `CameraCapture.SaveFolder` trong `setting.json`
- **Mẫu lỗ định vị:** thư mục qua `setting.json` → FiducialHoles
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
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/Camera/BaslerCameraService.cs` |
| Fiducial holes | `Processing/Fiducial/FiducialHoleDetectionService.cs`, `Views/Windows/FiducialTemplateWindow.xaml.cs` |
| Segmentation pipeline | `Processing/Segmentation/PcbSegmentationService.cs` |
| Basler parameters | `Processing/Camera/ICameraParameterService.cs`, `Config/setting.json` → CameraBasler |
| Cấu hình app | `Processing/Configuration/AppSettingService.cs`, `Models/Configuration/AppSetting.cs` |
