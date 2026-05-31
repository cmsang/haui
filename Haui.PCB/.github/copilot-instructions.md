# Copilot Instructions

> **Important:** This file must be read and fully applied every time Copilot performs any task in this workspace.

## Workspace & Project Info
- **Workspace root:** `D:\haui\Haui.PCB\`
- **Target framework:** .NET 10

## Architecture Principles (SOLID)
- **S**ingle Responsibility: each class has only one reason to change
- **O**pen/Closed: open for extension, closed for modification
- **L**iskov Substitution: subtypes must be substitutable for their supertypes
- **I**nterface Segregation: do not force classes to implement interfaces they do not need
- **D**ependency Inversion: depend on abstractions, not on concrete implementations

## Coding Guidelines
- **Separate main logic from UI forms**: all business logic must reside in ViewModel or Service; Form/Window should only contain UI event handling code and control updates
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
- Use `using` declarations (C# 8+) for all `IDisposable` objects
- Prefer `using var` over `using()` blocks for readability
- Use file-scoped namespaces (`namespace Foo.Bar;`)

## File & Namespace Conventions
- File-scoped namespaces are mandatory: `namespace Haui.PCB.SomeModule;`
- File name must match class name
- Each file contains only one main class/interface/enum

## Tool & Library Preferences
- AForge.NET for image and video processing
- Apply the `IDisposable` pattern correctly for all disposable objects
- Do not add new libraries unless absolutely necessary
