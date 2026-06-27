# Tech Context — Haui.PCB

## Stack

| Layer | Technology |
|-------|------------|
| UI | WPF (.NET 10 Windows) |
| Camera | Basler pylon (`Basler.Pylon.dll`, x64) |
| Vision | OpenCvSharp4 4.13 + WpfExtensions + runtime.win |

## Build and run

From workspace root (see `AGENTS.md`):

```bash
dotnet build Haui.PCB.slnx
dotnet run --project Haui.PCB/Haui.PCB.csproj
```

**Requirements:** Windows x64, .NET 10 SDK, Basler pylon installed.

Runtime **working directory** = process CWD (typically `bin/Debug/net10.0-windows/` when run from IDE). All data paths below are relative to CWD.

## Basler camera (detailed reference)

Connection flow, stream/buffer settings, GenICam parameters, official pylon samples, and community WPF patterns → **`memory-bank/baslerCamera.md`**.

Summary for Haui.PCB:

| Item | Value |
|------|-------|
| Service | `Processing/Camera/BaslerCameraService.cs` |
| DLL | `$(PylonRoot)\Development\Assemblies\Basler.Pylon\net8.0\x64\Basler.Pylon.dll` |
| Defaults | `Config/setting.json` → `CameraBasler` |
| Grab | `LatestImages` + `GrabLoop.ProvidedByStreamGrabber` → OpenCV `Mat` BGR8 |

## Runtime data files

| Path | Written by | Purpose |
|------|------------|---------|
| `last_region.json` | `MainViewModel` | Last camera ROI (pixel rect) |
| `Config/setting.json` | `AppSettingService` | Robot, `DatabaseConnection`, `ComponentDetection`, `PcbBoard`, `CameraBasler`, `CameraCapture` |
| `ComponentDetection.modelPath` | ONNX runtime | `Models/yolo26m_960x1280.onnx` (or custom path) |

## Segmentation constants (`PcbSegmentationService`)

| Constant | Value | Purpose |
|----------|-------|---------|
| `CannyThreshold1/2` | 30 / 100 (default) | Edge detection — section `Segmentation` in `setting.json`; chỉ sửa trực tiếp trong file (không có trong tab Cài đặt) |
| `MorphKernelSize` | 5 | Close gaps in edges |
| `EdgePadding` | 2 px | Crop padding |

Pipeline: BGR→gray → GaussianBlur(5×5) → Canny → morphology close → holder contour quad (460×590 mm) → perspective warp → landscape normalize.

## Comparison (YOLO — `MissingComponentDetectionService`)

- Input: warped board `Mat` (Mono8 or BGR) after `PcbSegmentationService`
- Model: ONNX YOLO26, default `Models/yolo26m_960x1280.onnx`, letterbox 960×1280
- Each box above **`confThreshold`** = one **missing** component; **PASS** when no boxes after NMS
- Class labels from `ComponentDetection.classNames` (11 classes, matches `yolo/yolo_dataset/data.yaml`)

## Models (inspection)

- `MissingComponent` — `Label`, `Confidence`, `Box`
- `ComponentInspectionResult` — `Missing`, `IsComplete`
- `PipelineStep` — debug step label + frozen `BitmapSource`

## Do not index / edit

- `Haui.PCB/bin/`, `Haui.PCB/obj/`, `.vs/`

## Adding dependencies

Avoid new NuGet packages unless necessary — project standard is OpenCvSharp + Basler pylon.
