# Copilot Instructions

## Architecture Principles (SOLID)

## Coding Guidelines
- Separate UI logic from processing and ML logic
- Avoid putting business logic inside Form classes
- Use async/await for long-running operations
- Keep methods small and testable
- Add Vietnamese comments for complex logic
- Follow OOP principles

## Code Style
- PascalCase for public methods
- camelCase for local variables and private fields (prefix `_` for private fields)
- Meaningful variable names
- Avoid magic numbers — use `const` or `readonly` fields
- Use `using` declarations (C# 8+) for all `IDisposable` OpenCvSharp objects (`Mat`, `VideoCapture`, `BackgroundSubtractor`, etc.)
- Prefer `using var` over `using()` blocks for readability
- Never pass a disposed `Mat` to downstream methods
- Use file-scoped namespaces (`namespace Foo.Bar;`)
