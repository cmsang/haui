# Haui.PCB — Agent Guide

WPF desktop app for **PCB inspection**: camera capture → OpenCV board segmentation → template regions → histogram-based region comparison (`Config/setting.json` → `ComponentTemplates.MinMatchSimilarityPercent`, default 80%).

## Context files (read first)

| Resource | Purpose |
|----------|---------|
| [memory-bank/projectbrief.md](memory-bank/projectbrief.md) | Mission, scope, workspace paths |
| [memory-bank/systemPatterns.md](memory-bank/systemPatterns.md) | Architecture, services, window graph, template gotcha |
| [memory-bank/techContext.md](memory-bank/techContext.md) | Stack, build/run, data paths, CV constants |
| [memory-bank/activeContext.md](memory-bank/activeContext.md) | Current focus and open decisions |
| [memory-bank/progress.md](memory-bank/progress.md) | What works, limitations, roadmap |
| [memory-bank/productContext.md](memory-bank/productContext.md) | Operator workflow, Vietnamese UI |
| [.github/copilot-instructions.md](.github/copilot-instructions.md) | SOLID, code style, library policy |
| [.cursor/rules/](.cursor/rules/) | Cursor session rules |

## Build and run

From workspace root:

```bash
dotnet build Haui.PCB.slnx
dotnet run --project Haui.PCB/Haui.PCB.csproj
```

## Project layout

- Solution: `Haui.PCB.slnx`
- App project: `Haui.PCB/` (WPF, `net10.0-windows`)
- Models: `Haui.PCB/Models/{Configuration,Templates,Segmentation,Camera,Robot}/`
- Services: `Haui.PCB/Processing/{Configuration,Camera,Segmentation,Templates,Fiducial,Robot}/`
- ViewModels: `Haui.PCB/ViewModels/` (+ `Pipeline/` cho `PipelineStep`)
- `GlobalUsings.cs` — namespace con của Models/Processing
- Views: `Haui.PCB/Views/Windows/`, `Views/Tabs/`, `Views/Controls/`; shell: `Views/Windows/MainWindow.xaml`

## Do not edit

- `Haui.PCB/bin/`, `Haui.PCB/obj/`, `.vs/`

## Extension points

1. New capability → `Processing/{Domain}/IMyService.cs` + implementation
2. Register in the window that needs it: `new MyViewModel(new MyService(), …)`
3. UI strings and labels: Vietnamese

## Template data

- Thư mục thư viện (`templates/` hoặc tùy chỉnh): mỗi mẫu = `*.png` + `*_regions.json`
- **Test / Create / Viewer** → `ITemplateLibraryService` (không dùng `template_board`)

After tasks, update `memory-bank/activeContext.md` and `memory-bank/progress.md`.
