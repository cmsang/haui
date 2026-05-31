# Progress — Haui.PCB

## Working features

- [x] DirectShow camera enumeration and live preview with FPS
- [x] Optional ROI selection on main preview (`last_region.json`)
- [x] PCB segmentation (rotated rect → perspective-corrected board image)
- [x] Pipeline step debugger window (Test 2)
- [x] Create template: draw regions on segmented board, save PNG + JSON
- [x] Multi-template library (`templates/index.json`) with viewer and edit/delete
- [x] Test pipeline: segment + compare regions vs single active template
- [x] Region overlay visualization (match green / mismatch red)
- [x] Sharpest-frame selection in camera service (Laplacian variance)

## Known limitations

- **No DI** — services wired manually in each window; duplicates instances
- **Dual template systems** — Test uses single-template files; library is separate
- **Paths relative to CWD** — running from different folders breaks saved templates
- **No unit/integration tests** in repository
- **No persistence** of comparison history or export

## Suggested next improvements

1. Let Test pipeline select a template from `TemplateLibraryService`
2. Centralize data directory (e.g. `Environment.SpecialFolder.ApplicationData`)
3. Add `App.xaml.cs` service registration for shared `ICameraService` lifetime
4. Export comparison results (CSV/JSON) from `TestPipelineViewModel`
5. Configurable match threshold (currently hardcoded 80% in `RegionComparisonResult`)

## Build status

Last documented command: `dotnet build Haui.PCB.slnx` — verify after major changes.
