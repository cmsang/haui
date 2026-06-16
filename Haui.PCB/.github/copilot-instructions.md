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

## Design Principles (DRY & KISS)

### DRY — Don't Repeat Yourself
- **One source of truth**: if the same rule, formula, or workflow appears in more than one place, extract it to a shared method, service, or constant.
- **Reuse existing abstractions** before adding new ones — e.g. `ITemplateLibraryService`, processing helpers in `Processing/`, shared ViewModel base logic.
- **Do not copy-paste** across ViewModels, windows, or tabs (Test / Create / Viewer share one template library — wire through services, not duplicated paths).
- Extract only when repetition is **real and stable**; a one-off similarity is not enough reason for a new abstraction.

```csharp
// ❌ Same segmentation steps duplicated in multiple ViewModels
using var gray = new Mat();
Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
// ... threshold, morphology, warp repeated in each caller

// ✅ Shared pipeline via PcbSegmentationService
var segmentation = new PcbSegmentationService(_pipelineParameters);
using var pipeline = segmentation.RunPipeline(source);
```

```csharp
// ❌ Each window resolves template folder independently
var folder = string.IsNullOrEmpty(settings.CustomFolder) ? "templates" : settings.CustomFolder;

// ✅ Single path via ITemplateLibraryService
var templates = await _templateLibrary.LoadAllAsync();
```

### KISS — Keep It Simple, Stupid
- **Prefer the simplest solution that meets the requirement**; add layers, patterns, or generics only when the problem actually needs them.
- **Small, readable methods** over clever one-liners; explicit flow over heavy indirection.
- **Do not over-engineer**: no extra interfaces, factories, or wrappers for a single call site or hypothetical future use.
- **Do not under-engineer duplication**: when the same non-trivial block appears twice, DRY applies — balance simplicity with maintainability.

```csharp
// ❌ Factory + strategy for one fixed histogram comparison path
IComparisonStrategy strategy = _comparisonFactory.Create(ComparisonKind.Histogram);
var results = strategy.Compare(templateBoard, newBoard, regions);

// ✅ Inject and call the existing service
var results = _comparisonService.Compare(templateBoard, newBoard, regions);
```

```csharp
// ❌ Premature abstraction — helper used once, name hides intent
var x = NormalizeAndThreshold(src, 0.42, 127);

// ✅ Inline or a well-named local step when logic is short and used once
using var gray = new Mat();
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
Cv2.Threshold(gray, gray, 127, 255, ThresholdTypes.Binary);
```

**When DRY and KISS conflict:** extract shared logic only when duplication is clear and likely to stay; keep the extracted API small and obvious (one method, one service) rather than building a framework.

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
