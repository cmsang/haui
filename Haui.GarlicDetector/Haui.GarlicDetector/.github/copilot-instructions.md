# Project Context
This is a C# WinForms application for object recognition using Support Vector Machine (SVM).

# Tech Stack
- .NET Framework 4.8 or .NET 6 WinForms
- Language: C#
- Image Processing: EmguCV (OpenCV wrapper)
- Machine Learning: SVM (Emgu.CV.ML)
- UI: Windows Forms

# Architecture Principles (SOLID)
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
  
# Coding Guidelines
- Separate UI logic from processing and ML logic
- Avoid putting business logic inside Form classes
- Use async/await for long-running operations
- Keep methods small and testable
- Add vietnamese comments for complex logic
- Follow OOP principles

# ML Pipeline
- Input: image or feature vector
- Preprocessing: grayscale, resize, normalize
- Segmentation: HSV/Lab threshold + morphology
- Feature extraction: HOG, Hu Moments, shape features
- Model: SVM (linear or RBF)
- Output: predicted label
- Avoid blocking UI thread

# Expected Structure
- Interfaces:
  - IImageProcessor
  - IFeatureExtractor
  - IClassifier

- Implementations:
  - HsvImageProcessor
  - HogFeatureExtractor
  - SvmClassifier

# Code Style
- PascalCase for public methods
- Meaningful variable names
- Avoid magic numbers
- Dispose unmanaged resources (EmguCV objects)

# Constraints
- Do not generate unnecessary UI code
- Focus on core logic
- Keep classes loosely coupled and highly cohesive

