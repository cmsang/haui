# Copilot Instructions

## Project Context
This is a C# WinForms application for **garlic quality detection** using Support Vector Machine (SVM).  
The system segments garlic bulbs from images and classifies each detected bulb into one of three categories:
- **Tỏi to** (Large garlic) — label `0`
- **Tỏi nhỏ** (Small garlic) — label `1`
- **Tỏi hỏng** (Damaged/defective garlic) — label `2`

## Tech Stack
- Runtime: .NET 10 (`net10.0-windows`)
- Language: C# 13 (`Nullable enable`, `ImplicitUsings enable`)
- Image Processing: `OpenCvSharp4` v4.13.0 (OpenCV 4.13 wrapper)
- Image Processing Extensions: `OpenCvSharp4.Extensions` v4.13.0 (Bitmap ↔ Mat conversion)
- Machine Learning: SVM via `OpenCvSharp.ML` (`OpenCvSharp.ML.SVM`)
- UI: Windows Forms (`UseWindowsForms`)

## Domain Knowledge — Garlic Classification
- **Tỏi to (Large)**: large contour area, roughly circular/oval shape, uniform bright color (whitish/cream)
- **Tỏi nhỏ (Small)**: small contour area, similar shape to large but smaller bounding box
- **Tỏi hỏng (Damaged)**: irregular shape, dark spots, discoloration, shriveled or broken appearance
- Segmentation should isolate individual garlic bulbs from background using HSV/Lab color thresholding + morphological operations
- Size classification is based on contour area or bounding box dimensions relative to a calibrated threshold
- Damage detection relies on texture irregularity (HOG), color anomaly (dark/brown regions), and shape deformation

## Classification Labels
| Label | Vietnamese | Description |
|-------|-----------|-------------|
| `0`   | Tỏi to    | Large garlic bulb |
| `1`   | Tỏi nhỏ   | Small garlic bulb |
| `2`   | Tỏi hỏng  | Damaged / defective garlic bulb |

## Architecture Principles (SOLID)
Follow SOLID principles strictly when generating code:

1. Single Responsibility Principle (SRP)
- Each class should have only one responsibility.
- Example:
  - `GarlicSegmentor`: only handles image segmentation and contour extraction
  - `GarlicFeatureExtractor`: only extracts features (HOG, Hu Moments, area, color stats)
  - `SvmClassifier`: only handles training and prediction

2. Open/Closed Principle (OCP)
- Code should be open for extension but closed for modification.
- Use interfaces or abstract classes for extensibility.
- Example:
  - `IFeatureExtractor` → allows adding HOG, SIFT, or custom features without modifying existing code

3. Liskov Substitution Principle (LSP)
- Derived classes must be replaceable for base classes without breaking functionality.
- Ensure all implementations of interfaces behave consistently.

4. Interface Segregation Principle (ISP)
- Do not create large, general-purpose interfaces.
- Split into smaller, specific interfaces.
- Example:
  - `ITrainer`
  - `IPredictor`
  - `IPreprocessor`

5. Dependency Inversion Principle (DIP)
- Depend on abstractions, not concrete implementations.
- Use dependency injection where appropriate.
- Example:
  - `SvmClassifier` depends on `IFeatureExtractor` instead of a concrete class

## Coding Guidelines
- Separate UI logic from processing and ML logic
- Avoid putting business logic inside Form classes
- Use async/await for long-running operations
- Keep methods small and testable
- Add Vietnamese comments for complex logic
- Follow OOP principles

## ML Pipeline
1. **Input**: raw image (from file or camera)
2. **Preprocessing**: convert to HSV/Lab, resize, normalize
3. **Segmentation**: HSV/Lab threshold → binary mask → morphological open/close → find contours → extract each garlic ROI
4. **Feature extraction per ROI**:
   - HOG descriptor (texture)
   - Hu Moments (shape)
   - Contour area & perimeter (size)
   - Mean color and standard deviation in HSV (color health)
5. **Classification**: SVM (RBF kernel) predicts label `0` (tỏi to), `1` (tỏi nhỏ), or `2` (tỏi hỏng)
6. **Output**: annotated image with bounding boxes labeled as "Tỏi to", "Tỏi nhỏ", or "Tỏi hỏng"
- Avoid blocking UI thread

## Expected Structure
- Interfaces:
  - `IImagePreprocessor`
  - `IGarlicSegmentor`
  - `IFeatureExtractor`
  - `IClassifier`

- Implementations:
  - `HsvGarlicPreprocessor` — grayscale/HSV preprocessing
  - `GarlicSegmentor` — contour-based garlic bulb segmentation
  - `GarlicFeatureExtractor` — HOG + Hu Moments + area + color features
  - `SvmClassifier` — SVM train/predict with labels 0, 1, 2

- Models / DTOs:
  - `GarlicRegion` — holds contour, bounding rect, ROI Mat, and predicted label
  - `GarlicLabel` enum: `ToTo = 0`, `ToNho = 1`, `ToHong = 2`

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

