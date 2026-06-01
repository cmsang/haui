# Product Context — Haui.PCB

## Why this exists

Operators need a Windows tool to **inspect PCB boards** against a visual template: capture a board under a camera, auto-crop/straighten it, then check whether defined regions (components, pads, traces) look similar to a reference image.

## Users

- Lab/line operators using a USB camera pointed at a work surface
- Developers tuning segmentation thresholds and comparison behavior

## UI language

- **Vietnamese** labels and status messages throughout WPF windows
- Code comments for complex logic are often Vietnamese (keep that style when extending)

## Primary workflow

1. **MainWindow** — select camera and resolution (default prefers 1280×720), live preview with FPS
2. **Optional ROI** — drag a rectangle on the preview; saved to `last_region.json` for focused capture
3. **Toolbar actions** (capture current frame and open child window):
   - **Test** → `TestPipelineWindow` — segment board, compare against **all** library templates, pick best match for detail view
   - **Test 2** → `PipelineStepsWindow` — visualize intermediate OpenCV steps for debugging
   - **Tạo mẫu** → `CreateTemplateWindow` — segment board, draw regions, save template
   - **Xem mẫu** → `TemplateViewerWindow` — browse multi-template library, edit/delete

## Comparison semantics

- Each `TemplateRegion` is compared via grayscale histogram correlation (`CompareHist` Correl)
- Similarity displayed as 0..100%
- **`IsMatch` when similarity ≥ 80%** (`RegionComparisonResult.IsMatch`)
- UI splits results into “giống” (matched) and “khác” (different) collections

## Template storage (user-visible)

Một thư viện trong thư mục cấu hình (`templates/` hoặc tùy chỉnh): mỗi mẫu = PNG + file `*_regions.json`. Create, Viewer và Test dùng chung thư viện.
