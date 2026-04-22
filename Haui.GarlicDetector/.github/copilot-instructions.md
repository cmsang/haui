# Copilot Instructions

## Project Context
This is a C# WinForms application for **garlic quality detection** using Support Vector Machine (SVM).  
The system segments garlic bulbs from images and classifies each detected bulb using a **two-stage pipeline**:
1. **SVM** classifies whether a bulb is **tỏi bình thường** (normal) or **tỏi hỏng** (damaged)
2. If the bulb is **normal**, its **pixel size (contour area / bounding box)** determines whether it is **tỏi to** (large) or **tỏi nhỏ** (small)

Final output labels:
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

### Stage 1 — SVM: Normal vs Damaged
- **SVM input features per ROI**: Color features (HSV/LAB), LBP texture, shape descriptors, statistical features
- **Tỏi bình thường (Normal)**: regular shape, uniform bright/cream color, smooth texture → SVM label `0`
- **Tỏi hỏng (Damaged)**: irregular shape, dark/brown spots, discoloration, shriveled or broken → SVM label `1`
- Damage detection relies on:
  - **Color anomaly**: abnormal pixel ratios (green/purple/dark masks in HSV), LAB color stats
  - **Texture irregularity**: Uniform LBP histogram (59 bins)
  - **Shape deformation**: circularity, convexity, aspect ratio from contour
  - **Statistical complexity**: per-channel entropy, gradient magnitude (Sobel SNR)

### Stage 2 — Size Rule: Large vs Small (normal bulbs only)
- After SVM confirms a bulb is **normal**, apply a pixel-area threshold on the contour area (or bounding box dimensions)
- **Tỏi to (Large)**: contour area ≥ `SIZE_THRESHOLD_PX` → final label `0`
- **Tỏi nhỏ (Small)**: contour area < `SIZE_THRESHOLD_PX` → final label `1`
- `SIZE_THRESHOLD_PX` is a calibrated `const` (e.g. `5000` px²), adjustable per camera/setup

### Segmentation
- Isolate individual garlic bulbs from background using HSV/Lab color thresholding + morphological open/close operations
- Each contour that passes the minimum area filter is treated as one garlic ROI

## Classification Labels

### SVM output (Stage 1)
| SVM Label | Meaning | Description |
|-----------|---------|-------------|
| `0` | Tỏi bình thường | Normal garlic bulb (proceed to Stage 2) |
| `1` | Tỏi hỏng | Damaged / defective garlic bulb (final) |

### Final output labels (after Stage 2 size check)
| Final Label | Vietnamese | Description |
|-------------|-----------|-------------|
| `0` | Tỏi to | Large normal garlic bulb (area ≥ threshold) |
| `1` | Tỏi nhỏ | Small normal garlic bulb (area < threshold) |
| `2` | Tỏi hỏng | Damaged / defective garlic bulb |

## Architecture Principles (SOLID)
Follow SOLID principles strictly when generating code:

1. Single Responsibility Principle (SRP)
- Each class should have only one responsibility.
- Example:
  - `GarlicSegmentor`: only handles image segmentation and contour extraction
  - `GarlicFeatureExtractor`: only extracts features (Color, LBP, Shape, Statistical)
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
4. **Feature extraction per ROI** — `GarlicFeatureExtractor.Extract(Mat croppedGarlic)` (all features min-max normalized to [0,1], `TARGET_SIZE = 64`):
   - **Group 1 — Color Features** (~20 features):
     - HSV mean & std (H÷180, S÷255, V÷255)
     - Abnormal pixel ratios: green mask (H=35–85), purple mask (H=100–160), dark mask (V<60)
     - LAB mean & std (L/A/B ÷255)
     - HSV-H histogram (8 bins, range 0–180)
   - **Group 2 — LBP Texture Features** (~59 features):
     - Uniform Local Binary Pattern histogram (59 bins) on grayscale
     - Uniform patterns → bin = count of set bits (0–8); non-uniform → bin 58
     - Histogram normalized by total pixel count
   - **Group 3 — Shape Features** (~6 features):
     - From largest contour in Otsu-thresholded grayscale ROI
     - Area (÷TARGET_SIZE²), Circularity (4π·area/perimeter²), Convexity (area/hull area)
     - Aspect ratio (width/height), Perimeter (÷4·TARGET_SIZE), Contour point count (÷4·TARGET_SIZE)
     - Padding with zeros if no contour found
   - **Group 4 — Statistical Features** (~12 features):
     - Per BGR channel: mean (÷255), std (÷255), entropy (16-bin histogram, normalized by log(16))
     - Gradient magnitude via Sobel: mean (÷255), std (÷255), SNR (mean/(std+ε))
5. **Stage 1 — SVM classification** (RBF kernel):
   - Predicts `0` = tỏi bình thường (normal) or `1` = tỏi hỏng (damaged)
   - If damaged → assign final label `2` (Tỏi hỏng), skip Stage 2
6. **Stage 2 — Size-based classification** (normal bulbs only):
   - Compute contour area (px²) from the segmented ROI
   - If area ≥ `SIZE_THRESHOLD_PX` → final label `0` (Tỏi to)
   - If area < `SIZE_THRESHOLD_PX` → final label `1` (Tỏi nhỏ)
7. **Output**: annotated image with bounding boxes labeled as "Tỏi to", "Tỏi nhỏ", or "Tỏi hỏng"
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
  - `GarlicFeatureExtractor` — Color + LBP + Shape + Statistical features (TARGET_SIZE=64, ~97 features total, min-max normalized)
  - `SvmClassifier` — SVM train/predict with labels 0, 1, 2

- Models / DTOs:
  - `GarlicRegion` — holds contour, bounding rect, ROI Mat, contour area (px²), SVM prediction, and final label
  - `GarlicLabel` enum: `ToTo = 0`, `ToNho = 1`, `ToHong = 2`
  - `SvmLabel` enum: `BinhThuong = 0`, `Hong = 1` (internal, Stage 1 only)

- Constants (in `GarlicConstants` static class):
  - `SIZE_THRESHOLD_PX` — pixel area threshold separating tỏi to from tỏi nhỏ (default `5000`)
  - `MIN_CONTOUR_AREA` — minimum contour area to be considered a garlic bulb (default `500`)

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
- **Only read, create, or modify files inside `D:\haui\Haui.GarlicDetector\`**
- Do not generate unnecessary UI code
- Focus on core logic
- Keep classes loosely coupled and highly cohesive
- Do not use `Emgu.CV` — project uses `OpenCvSharp4` exclusively

