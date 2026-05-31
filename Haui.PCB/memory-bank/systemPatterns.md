# System Patterns — Haui.PCB

## Folder structure

```
Haui.PCB/                 # WPF app project
├── App.xaml(.cs)         # Startup → MainWindow (no composition root)
├── MainWindow.xaml(.cs)  # Camera hub
├── Processing/           # Services + I* interfaces
├── Models/               # DTOs (TemplateRegion, TemplateEntry, …)
├── ViewModels/           # INotifyPropertyChanged, business logic
└── Views/                # Secondary windows + thin code-behind
```

## Architecture

- **MVVM-lite**: ViewModels hold state and commands; Views subscribe to events / `PropertyChanged`
- **DIP without DI container**: depend on `I*` interfaces; concrete services constructed with `new` in window constructors
- **SOLID** enforced by convention — see `.github/copilot-instructions.md`

## Service interfaces (`Processing/`)

| Interface | Implementation | Role |
|-----------|----------------|------|
| `ICameraService` | `BaslerCameraService` | Basler pylon; frames as `Mat` |
| `ICameraParameterService` | `BaslerCameraService` | GenICam Apply / Reset (Basler only) |
| `IPcbSegmentationService` | `PcbSegmentationService` | Canny + contour + perspective warp → straight board |
| `ITemplateRegionService` | `TemplateRegionService` | Single active template JSON + PNG |
| `ITemplateLibraryService` | `TemplateLibraryService` | Multi-template `templates/` catalog |
| `IRegionComparisonService` | `RegionComparisonService` | Per-region histogram compare |
| `IPipelineDebugService` | `PipelineDebugService` | Step images for PipelineStepsWindow |

## Dual template storage (critical)

| System | Files | Used by |
|--------|-------|---------|
| Single (legacy/active) | `template_board.png`, `template_regions.json` | `TestPipelineWindow` / `TestPipelineViewModel` |
| Library | `templates/index.json`, `templates/{name}_{timestamp}.png` | `CreateTemplateWindow`, `TemplateViewerWindow` |

Creating a template in **Create** may write **both**. **Test** only reads the single-template files — not the library index.

## Window graph

```
App → MainWindow (MainViewModel + CameraService)
        ├─ TestPipelineWindow      (Owner=Main)
        ├─ PipelineStepsWindow     (Owner=Main)
        ├─ CreateTemplateWindow    (Owner=Main or Viewer)
        └─ TemplateViewerWindow    (Owner=Main)
              └─ CreateTemplateWindow (edit mode via RegionsSaved)
```

Child windows receive a captured `Mat` from Main; caller **disposes** the frame after handoff.

## Threading / WPF

- Camera callbacks are not on UI thread — use `Dispatcher.InvokeAsync` in code-behind for bindings
- Convert `Mat` → `BitmapSource` via `OpenCvSharp.WpfExtensions.BitmapSourceConverter`, then **`.Freeze()`** before binding

## OpenCV resource pattern

```csharp
using var gray = new Mat();
// … always dispose Mats cloned for pipeline branches
```

Never hold long-lived `Mat` on ViewModel without clear ownership; prefer `BitmapSource` for UI.

## Extension pattern

1. Add `IMyService` + implementation in `Processing/`
2. Instantiate in the window that needs it (same as `MainWindow.xaml.cs` line ~27)
3. Inject into ViewModel via constructor
