# Haui.PCB — Agent Guide

WPF desktop app for **PCB inspection**: camera → OpenCV segmentation → template regions → histogram comparison.

## Read order

| When | File | Content |
|------|------|---------|
| Always (non-trivial work) | [memory-bank/projectbrief.md](memory-bank/projectbrief.md) | Mission, scope, success criteria |
| Always | [memory-bank/systemPatterns.md](memory-bank/systemPatterns.md) | Architecture, services, windows, template gotcha |
| Always | [memory-bank/techContext.md](memory-bank/techContext.md) | Stack, build, data paths, CV constants |
| Always | [memory-bank/activeContext.md](memory-bank/activeContext.md) | Current focus, open decisions, entry files |
| As needed | [memory-bank/progress.md](memory-bank/progress.md) | Feature checklist, limitations |
| UX / operator flows | [memory-bank/productContext.md](memory-bank/productContext.md) | Vietnamese UI, workflows |
| Camera / Basler pylon work | [memory-bank/baslerCamera.md](memory-bank/baslerCamera.md) | Connection, settings, official & community samples |
| Writing C# | [.github/copilot-instructions.md](.github/copilot-instructions.md) | SOLID, style, library policy |

Cursor rules: [.cursor/rules/](.cursor/rules/) — bootstrap, `language-and-ui-text` (English comments, Vietnamese UI/font), glob rules for `*.cs`.

## Build and run

From workspace root:

```bash
dotnet build Haui.PCB.slnx
dotnet run --project Haui.PCB/Haui.PCB.csproj
```

Requirements: Windows x64, .NET 10 SDK, Basler pylon. Details → `memory-bank/techContext.md`.

## Critical gotcha

**Test / Create / Viewer** share one template library (`*.png` + `*_regions.json` per sample). Path → `Config/setting.json` → `ComponentTemplates.CustomFolder` (empty → `templates/`). All use `ITemplateLibraryService`.

## After tasks

Update only `memory-bank/activeContext.md` and `memory-bank/progress.md` — small edits, no full rewrites.
