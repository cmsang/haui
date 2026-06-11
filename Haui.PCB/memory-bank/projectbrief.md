# Project Brief — Haui.PCB

## Mission

Desktop WPF application for **PCB board inspection**: live camera capture, automatic PCB segmentation from background, template region definition, and region-by-region similarity comparison against a saved template.

## Goals

- Capture frames from Basler cameras (pylon SDK)
- Detect and straighten rotated rectangular PCBs (OpenCV contour + perspective warp)
- Let operators define named regions on a segmented board (relative 0..1 coordinates)
- Compare new boards to a template using histogram correlation (threshold via `Config/setting.json` → `ComponentTemplates.MinMatchSimilarityPercent`, default 80%)
- Support debug visualization of the segmentation pipeline step-by-step

## Non-Goals (current scope)

- No cloud/backend integration
- No ML/YOLO detection in this project
- No dependency injection container (manual wiring only)
- No automated tests in repo yet

## Workspace

| Item | Path |
|------|------|
| Solution | `Haui.PCB.slnx` |
| Main project | `Haui.PCB/Haui.PCB.csproj` |
| Target framework | `net10.0-windows` (WPF) |

## Success criteria for changes

- Business logic stays in `ViewModels/` or `Processing/`, not in XAML code-behind beyond UI events
- All `OpenCvSharp.Mat` instances disposed via `using` / `using var`
- Follow `.github/copilot-instructions.md`
