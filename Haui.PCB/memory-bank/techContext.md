# Tech Context — Haui.PCB

## Stack

| Layer | Technology |
|-------|------------|
| UI | WPF (.NET 10 Windows) |
| Camera | AForge.Video.DirectShow 2.2.5 |
| Vision | OpenCvSharp4 4.13 + WpfExtensions + runtime.win |

## Build and run

From workspace root `D:\haui\Haui.PCB\`:

```bash
dotnet build Haui.PCB.slnx
dotnet run --project Haui.PCB/Haui.PCB.csproj
```

**Requirements:** Windows, .NET 10 SDK, USB camera (DirectShow).

Runtime **working directory** = process CWD (typically `bin/Debug/net10.0-windows/` when run from IDE). All data paths below are relative to CWD.

## Runtime data files

| Path | Written by | Purpose |
|------|------------|---------|
| `last_region.json` | `MainViewModel` | Last camera ROI (pixel rect) |
| `template_regions.json` | `TemplateRegionService` | Active template region list |
| `template_board.png` | `TemplateRegionService` | Active template board image |
| `templates/index.json` | `TemplateLibraryService` | Library catalog |
| `templates/*.png` | `TemplateLibraryService` | Per-template board images |

## Segmentation constants (`PcbSegmentationService`)

| Constant | Value | Purpose |
|----------|-------|---------|
| `CannyThreshold1/2` | 50 / 150 | Edge detection |
| `MorphKernelSize` | 5 | Close gaps in edges |
| `MinAreaRatio` | 0.01 | Min contour area vs image |
| `EdgePadding` | 2 px | Crop padding |

Pipeline: BGR→gray → GaussianBlur(5×5) → Canny → morphology close → largest external contour → `MinAreaRect` / quad → perspective warp → landscape normalize.

## Comparison (`RegionComparisonService`)

- Crop region on template and new board using relative coords
- Resize to 64×64, grayscale histogram, `CompareHist` with `HistCompMethods.Correl`
- Match threshold **80%** enforced in `RegionComparisonResult.IsMatch`

## Models

- `TemplateRegion` — `RelX`, `RelY`, `RelWidth`, `RelHeight` (0..1), `Name`
- `TemplateEntry` — library metadata + image path + regions
- `RegionComparisonResult` — similarity %, `BoardRect`, `IsMatch`
- `PipelineStep` — debug step label + frozen `BitmapSource`

## Do not index / edit

- `Haui.PCB/bin/`, `Haui.PCB/obj/`, `.vs/`

## Adding dependencies

Avoid new NuGet packages unless necessary — project standard is AForge + OpenCvSharp only.
