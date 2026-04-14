# Copilot Instructions

## Project Context
This is a C# WinForms application for object recognition using Support Vector Machine (SVM).

## Tech Stack
- Runtime: .NET 10 (`net10.0-windows`)
- Language: C# 13 (`Nullable enable`, `ImplicitUsings enable`)
- Image Processing: `OpenCvSharp4` v4.13.0 (OpenCV 4.13 wrapper)
- Image Processing Extensions: `OpenCvSharp4.Extensions` v4.13.0 (Bitmap ↔ Mat conversion)
- Machine Learning: SVM via `OpenCvSharp.ML` (`OpenCvSharp.ML.SVM`)
- UI: Windows Forms (`UseWindowsForms`)

## Architecture Principles (SOLID)
Follow SOLID principles strictly when generating code:

1. Single Responsibility Principle (SRP)
- Each class should have only one responsibility.
- Example:
  - ImageProcessor: only handles image preprocessing and segmentation
  - FeatureExtractor: only extracts features (HOG, Hu Moments, etc.)
  - SvmClassifier: only handles training and prediction

2. Open/Closed Principle (OCP)
- Code should be open for extension but closed for modification.
- Use interfaces or abstract classes for extensibility.
- Example:
  - IFeatureExtractor → allows adding HOG, SIFT, or custom features without modifying existing code

3. Liskov Substitution Principle (LSP)
- Derived classes must be replaceable for base classes without breaking functionality.
- Ensure all implementations of interfaces behave consistently.

4. Interface Segregation Principle (ISP)
- Do not create large, general-purpose interfaces.
- Split into smaller, specific interfaces.
- Example:
  - ITrainer
  - IPredictor
  - IPreprocessor

5. Dependency Inversion Principle (DIP)
- Depend on abstractions, not concrete implementations.
- Use dependency injection where appropriate.
- Example:
  - SvmClassifier depends on IFeatureExtractor instead of concrete class
  
## Coding Guidelines
- Separate UI logic from processing and ML logic
- Avoid putting business logic inside Form classes
- Use async/await for long-running operations
- Keep methods small and testable
- Add vietnamese comments for complex logic
- Follow OOP principles

## ML Pipeline
- Input: image or feature vector
- Preprocessing: grayscale, resize, normalize
- Segmentation: HSV/Lab threshold + morphology
- Feature extraction: HOG, Hu Moments, shape features
- Model: SVM (linear or RBF)
- Output: predicted label
- Avoid blocking UI thread

## Expected Structure
- Interfaces:
  - IImageProcessor
  - IFeatureExtractor
  - IClassifier

- Implementations:
  - HsvImageProcessor
  - HogFeatureExtractor
  - SvmClassifier

## Code Style
- PascalCase for public methods
- camelCase for local variables and private fields (prefix `_` for private fields)
- Meaningful variable names
- Avoid magic numbers — use `const` or `readonly` fields
- Use `using` declarations (C# 8+) for all `IDisposable` OpenCvSharp objects (`Mat`, `VideoCapture`, `BackgroundSubtractor`, etc.)
- Prefer `using var` over `using()` blocks for readability
- Never pass a disposed `Mat` to downstream methods
- Use file-scoped namespaces (`namespace Foo.Bar;`)

## Best Practices (OpenCvSharp4)
- Always dispose `Mat` objects — unmanaged memory is NOT collected by GC
- Prefer `Mat.Clone()` over direct assignment to avoid shared native pointers
- Use `Cv2.*` static methods instead of instance methods where available
- For WinForms display: convert `Mat` → `Bitmap` via `OpenCvSharp.Extensions.BitmapConverter.ToBitmap()`
- Use `Task.Run()` + `Invoke()` for camera loops to keep UI thread free
- Check `mat.Empty()` before processing to guard against null frames
- Use `InputArray` / `OutputArray` overloads for zero-copy operations

## Constraints
- Do not generate unnecessary UI code
- Focus on core logic
- Keep classes loosely coupled and highly cohesive
- Do not use `Emgu.CV` — project uses `OpenCvSharp4` exclusively

