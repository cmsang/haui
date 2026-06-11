# Haui.PCB

Windows WPF application for **PCB board inspection** using a Basler industrial camera and OpenCV.

## Features

- Live camera preview (Basler GigE/USB via pylon)
- Automatic PCB detection, crop, and perspective correction
- Define template regions on a segmented board
- Compare new captures to a template (histogram correlation, 80% match threshold)
- Debug view of the segmentation pipeline
- Multi-template library under `templates/`
- Basler GenICam parameter panel (Exposure, Gain, Gamma; Apply / Reset)

## Requirements

- Windows 10/11 (x64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Basler camera with [pylon Camera Software Suite](https://www.baslerweb.com/en/downloads/software-downloads/) installed

### Basler setup

- Default SDK path: `C:\Program Files\Basler\pylon`
- Build targets **x64** and references `Basler.Pylon.dll` from pylon Development folder
- Recommended parameters: `Haui.PCB/Config/setting.json` → `CameraBasler`

## Build and run

```bash
dotnet build Haui.PCB.slnx
dotnet run --project Haui.PCB/Haui.PCB.csproj
```

Template and ROI files are written relative to the **process working directory** (usually `Haui.PCB/bin/Debug/net10.0-windows/` when started from Visual Studio or `dotnet run`).

## Project structure

| Path | Description |
|------|-------------|
| `Haui.PCB/` | Main WPF application |
| `Haui.PCB/Processing/` | Camera, segmentation, template, comparison services |
| `Haui.PCB/ViewModels/` | Business logic |
| `Haui.PCB/Views/` | Secondary windows |
| `memory-bank/` | Architecture and progress docs for AI assistants |
| `AGENTS.md` | Cursor agent entry point |

## AI-assisted development

- **Memory Bank:** `memory-bank/` — start here for architecture and current state
- **Agent guide:** `AGENTS.md`
- **Coding rules:** `.github/copilot-instructions.md` and `.cursor/rules/`

## License

Internal HAUI project — see repository root for license if applicable.
