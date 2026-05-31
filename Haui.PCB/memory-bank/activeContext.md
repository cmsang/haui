# Active Context — Haui.PCB

## Current focus

**Memory Bank initialized** — baseline documentation for Cursor/Copilot agents. No feature development in this session.

## Recent change

- Added `memory-bank/`, `.cursor/rules/`, `AGENTS.md`, `README.md`, `.cursorignore`
- Linked AI context from `.github/copilot-instructions.md`

## Open decisions

1. **Unify template storage** — Should `TestPipelineWindow` use `TemplateLibraryService` (pick template by name) instead of only `template_board.png` / `template_regions.json`?
2. **Composition root** — Optional future: register services in `App.xaml.cs` instead of per-window `new`
3. **CWD vs app data folder** — Templates currently live next to the executable; consider `%AppData%/Haui.PCB/` for production

## Files to read first for common tasks

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/CameraService.cs` |
| Segmentation tuning | `Processing/PcbSegmentationService.cs` |
| Comparison / threshold | `Processing/RegionComparisonService.cs`, `Models/RegionComparisonResult.cs` |
| Template CRUD | `Processing/TemplateRegionService.cs`, `Processing/TemplateLibraryService.cs` |
| Test UI flow | `ViewModels/TestPipelineViewModel.cs`, `Views/TestPipelineWindow.xaml.cs` |

## Session maintenance

After significant work, update this file (focus + decisions) and `progress.md` (what works / what’s next). Do not rewrite the whole Memory Bank each time.
