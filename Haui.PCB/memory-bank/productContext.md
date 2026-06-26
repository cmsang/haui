# Product Context — Haui.PCB

## Why this exists

Operators inspect PCB boards: capture under camera, auto-crop/straighten, then detect **missing component locations** with a trained YOLO model.

## Users

- Lab/line operators with a camera over a work surface
- Developers tuning segmentation and YOLO thresholds

## UI language

**Vietnamese** labels and status messages (correct diacritics; `MaterialDesignFont` / `Segoe UI`). Source comments: **English only** — see `.cursor/rules/language-and-ui-text.mdc`.

## Primary workflow

1. **MainWindow** — camera, resolution, live preview + FPS
2. **Optional ROI** — rectangle on preview → `last_region.json`
3. **Toolbar**:
   - **Test** → segment board, YOLO missing-component detection (camera capture)
   - **Chọn ảnh** (DeveloperMode) → same inspection from file; `ResultImage` shows annotated board
   - **Chụp** → save frame to `CameraCapture.SaveFolder`

## Inspection semantics

- YOLO ONNX on warped board image; **each detection box = one missing component**
- **PASS** when no missing locations detected (`MissingCount == 0`)
- **FAIL** when ≥1 box; red overlay + list in **Danh sách linh kiện thiếu**
- Thresholds: `ComponentDetection.confThreshold`, `iouThreshold` in `setting.json`

## Model assets

ONNX file (e.g. `yolo26m_960x1280.onnx`) under `Haui.PCB/Models/` or custom path in settings. Trained via `yolo/` (Ultralytics YOLO26).
