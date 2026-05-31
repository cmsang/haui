# Active Context — Haui.PCB

## Current focus

**Unified segmentation pipeline** — `RunPipeline()` in `PcbSegmentationService` is the single source of truth; `PipelineDebugService` only visualizes.

## Recent change

- `SegmentationPipelineResult` holds intermediate Mats + metadata
- `Segment()` delegates to `RunPipeline()`; debug window calls same path via `IPcbSegmentationService`

## Open decisions

1. **Unify template storage** — Should `TestPipelineWindow` use `TemplateLibraryService`?
2. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench

## Files to read first for common tasks

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/BaslerCameraService.cs` |
| Basler parameters | `Processing/ICameraParameterService.cs`, `camera_basler_defaults.json` |
