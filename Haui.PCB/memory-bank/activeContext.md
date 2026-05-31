# Active Context — Haui.PCB

## Current focus

**Basler-only camera** — removed DirectShow/AForge; app uses `BaslerCameraService` exclusively.

## Recent change

- Deleted `DirectShowCameraService`, `CameraDiscoveryService`, `CameraBackend`
- Removed NuGet: `AForge.Video.DirectShow`, `OpenCvSharp4.Extensions`
- `MainViewModel` wires `BaslerCameraService` directly

## Open decisions

1. **Unify template storage** — Should `TestPipelineWindow` use `TemplateLibraryService`?
2. **Calibrate defaults** — `camera_basler_defaults.json` for acA4600-7gc on real bench

## Files to read first for common tasks

| Task | Start here |
|------|------------|
| Camera / capture | `ViewModels/MainViewModel.cs`, `Processing/BaslerCameraService.cs` |
| Basler parameters | `Processing/ICameraParameterService.cs`, `camera_basler_defaults.json` |
