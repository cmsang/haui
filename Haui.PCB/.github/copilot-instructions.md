# Copilot / Cursor Coding Instructions

Project context → `AGENTS.md` and `memory-bank/`. This file is **coding standards only**.

## Architecture (SOLID)

- **S**ingle Responsibility — one reason to change per class
- **O**pen/Closed — extend via interfaces, not by editing consumers
- **L**iskov Substitution — subtypes substitutable for abstractions
- **I**nterface Segregation — small focused interfaces
- **D**ependency Inversion — depend on `I*` in `Processing/`; wire with `new` in window constructors (no DI container)

## MVVM

- Business logic in **ViewModel** or **Processing** services — never in XAML code-behind beyond UI events
- Windows construct services and inject into ViewModels (see `MainWindow.xaml.cs`)

## Code style

- File-scoped namespaces: `namespace Haui.PCB.Processing.Camera;`
- One main type per file; file name matches type name
- PascalCase public members; `_camelCase` private fields; camelCase locals
- `using var` for all `IDisposable` (especially `OpenCvSharp.Mat`)
- Avoid magic numbers — use `const` / `readonly`
- **Comments in English only** — operator UI strings stay Vietnamese (see `.cursor/rules/language-and-ui-text.mdc`)

## Libraries

- **OpenCvSharp** for image processing; **Basler pylon** for camera
- No new NuGet packages unless strongly justified

## Async

Use `async`/`await` for long operations; `Dispatcher.InvokeAsync` for UI updates from camera callbacks.
