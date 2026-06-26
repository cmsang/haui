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
| `Config/setting.json` | `AppSettingService` | Robot, `DatabaseConnection`, `ComponentTemplates`, `PcbBoard`, `CameraBasler`, `CameraCapture` |
| `ComponentTemplates.CustomFolder` (`setting.json`) | `TemplateLibraryService` | Thư mục thư viện mẫu (`*.png` + `*_regions.json`; rỗng → `templates/`) |

## Segmentation constants (`PcbSegmentationService`)

| Constant | Value | Purpose |
|----------|-------|---------|
| `CannyThreshold1/2` | 50 / 150 (default; chỉnh trên MainWindow) | Edge detection — `SegmentationSettings.Current` |
| `MorphKernelSize` | 5 | Close gaps in edges |
| `EdgePadding` | 2 px | Crop padding |

Pipeline: BGR→gray → GaussianBlur(5×5) → Canny → morphology close → holder contour quad (460×590 mm) → perspective warp → landscape normalize.

## Comparison (`RegionComparisonService`)

- Crop region on template and new board using relative coords
- Resize to **128×128** (`INTER_AREA` when downscaling, `INTER_LINEAR` when upscaling)
- Preprocess: **LAB L** → **CLAHE** (clip 2.0, tile 8×8) → **bilateral** (d=5)
- Similarity metric: **Hybrid score** = `0.7 * NCC(CCoeffNormed)` + `0.3 * histogram correlation`; both clamped to 0..1
- Match threshold **`MinMatchSimilarityPercent`** in `setting.json` → ComponentTemplates (default 80); `RegionComparisonResult.IsMatch`

## Models

- `TemplateRegion` — `RelX`, `RelY`, `RelWidth`, `RelHeight` (0..1), `Name`
- `TemplateEntry` — library metadata + image path + regions
- `RegionComparisonResult` — similarity %, `BoardRect`, `IsMatch`
- `PipelineStep` — debug step label + frozen `BitmapSource`

## Do not index / edit

- `Haui.PCB/bin/`, `Haui.PCB/obj/`, `.vs/`

## Adding dependencies

Avoid new NuGet packages unless necessary — project standard is OpenCvSharp + Basler pylon.
