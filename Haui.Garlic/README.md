# Haui.GarlicDetector — Hệ thống phân loại củ tỏi

Ứng dụng **WinForms (.NET 10)** nhận diện và phân loại củ tỏi theo thời gian thực từ camera hoặc ảnh tĩnh, sử dụng pipeline **tiền xử lý HSV → phân vùng màu sắc → trích xuất đặc trưng → SVM**.

---

## 1. Giới thiệu

Project phân loại củ tỏi thành **3 nhãn cuối**:

| Nhãn | Tên | Mô tả |
|------|-----|-------|
| `0` | **Tỏi to** | Củ bình thường, diện tích contour ≥ ngưỡng kích thước |
| `1` | **Tỏi nhỏ** | Củ bình thường, diện tích contour < ngưỡng kích thước |
| `2` | **Tỏi hỏng** | Củ bị lỗi, dập, mốc, thối, đổi màu bất thường |

Phân loại được thực hiện qua **2 giai đoạn nối tiếp**:

1. **Stage 1 — SVM:** phân biệt *bình thường* (label `0`) vs *hỏng* (label `1`)
2. **Stage 2 — Quy tắc kích thước:** nếu bình thường → so diện tích với `SizeThresholdPx` → ra nhãn *to* hoặc *nhỏ*

---

## 2. Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| Runtime | .NET 10 (`net10.0-windows`) |
| Ngôn ngữ | C# 13 (Nullable enable, ImplicitUsings) |
| Giao diện | Windows Forms |
| Xử lý ảnh | OpenCvSharp4 v4.13.0 (wrapper OpenCV 4.13) |
| Bitmap ↔ Mat | OpenCvSharp4.Extensions v4.13.0 |
| Học máy | `OpenCvSharp.ML.SVM` (C-SVC, kernel RBF) |
| Giao tiếp robot | `System.IO.Ports` (Serial/UART) |

---

## 3. Kiến trúc hệ thống

```
Haui.GarlicDetector/
├── Common/
│   └── AppSettings.cs          # Singleton cài đặt (settings.json), ngưỡng HSV, kích thước...
├── Models/
│   ├── GarlicLabel.cs          # Enum: ToTo=0, ToNho=1, ToHong=2
│   ├── GarlicRegion.cs         # Thông tin 1 củ: BoundingBox, Area, Circularity, FinalLabel
│   └── GarlicSegmentResult.cs  # Kết quả phân vùng: BoundingRect, Area, Circularity
├── Vision/
│   ├── IImagePreprocessor.cs   # Interface tiền xử lý
│   ├── HsvGarlicPreprocessor.cs# BGR → GaussianBlur → HSV
│   ├── IGarlicSegmentor.cs     # Interface phân vùng
│   └── HsvSegmenter.cs         # Phân vùng 2 mask HSV song song + morphology
├── ML/
│   ├── IFeatureExtractor.cs    # Interface trích xuất đặc trưng
│   ├── GarlicFeatureExtractor.cs # 4 nhóm đặc trưng, ~97 chiều
│   ├── IGarlicClassifier.cs    # Interface phân loại
│   └── SvmClassifier.cs        # SVM C-SVC RBF: train, save, load, predict
├── Services/
│   ├── CameraService.cs        # Capture ~30 fps qua DirectShow, CaptureSharpestFrame
│   └── GarlicPipeline.cs       # Điều phối camera + phân vùng async, auto/manual mode
└── Forms/
    ├── frmMain.cs              # UI chính: live feed, overlay, serial robot
    ├── frmTrainSvm.cs          # Giao diện huấn luyện SVM từ thư mục ảnh
    ├── frmLabeling.cs          # Gán nhãn thủ công ảnh chụp
    ├── frmSettings.cs          # Cài đặt HSV threshold, AutoDetect
    ├── frmRegionSelector.cs    # Vẽ vùng nhận diện (DetectionRegion)
    └── frmTestDetection.cs     # Test phân loại ảnh tĩnh
```

---

## 4. Pipeline xử lý chi tiết

```
Camera / Ảnh tĩnh
        │
        ▼
┌───────────────────────┐
│  Bước 1: Tiền xử lý  │  HsvGarlicPreprocessor
│  BGR → Blur → HSV    │
└──────────┬────────────┘
           │ Mat (HSV)
           ▼
┌───────────────────────┐
│  Bước 2: Phân vùng   │  HsvSegmenter
│  (2 mask HSV + OR)   │
└──────────┬────────────┘
           │ List<GarlicSegmentResult>
           ▼
┌───────────────────────┐
│  Bước 3: Crop ROI    │  Với mỗi BoundingRect
│  → Resize 64×64      │
└──────────┬────────────┘
           │ Mat (ROI 64×64)
           ▼
┌───────────────────────┐
│  Bước 4: Trích đặc   │  GarlicFeatureExtractor
│  trưng (~97 chiều)   │  Color + LBP + Shape + Stat
└──────────┬────────────┘
           │ float[97] (min-max normalized)
           ▼
┌───────────────────────┐
│  Bước 5: SVM Stage 1 │  SvmClassifier.Predict()
│  Bình thường / Hỏng  │  RBF kernel, C-SVC
└──────────┬────────────┘
           │
     ┌─────┴─────┐
     │ Hỏng (1)  │ Bình thường (0)
     ▼           ▼
  ToHong=2  ┌──────────────────────┐
            │  Bước 6: Stage 2     │
            │  Area ≥ threshold?   │
            └──────┬───────────────┘
              ┌────┴────┐
           Có (≥)    Không (<)
              ▼         ▼
           ToTo=0   ToNho=1
```

---

## 5. Chi tiết từng bước

### Bước 1 — Tiền xử lý (`HsvGarlicPreprocessor`)

```
BGR frame  →  GaussianBlur(5×5)  →  CvtColor(BGR2HSV)  →  Mat HSV
```

- **Gaussian Blur** kernel 5×5 (mặc định, cấu hình được) làm mịn nhiễu *salt-and-pepper* trước khi chuyển màu, tránh pixel nhiễu tạo ra vùng phân vùng giả.
- Output là **Mat HSV** — đầu vào duy nhất cho `HsvSegmenter`.
- Kernel size = 0 hoặc 1 → bỏ qua blur (dùng khi ảnh đã sạch hoặc cần tốc độ tối đa).

---

### Bước 2 — Phân vùng HSV (`HsvSegmenter`)

Phân vùng dùng **2 ngưỡng HSV song song** để bắt được cả tỏi lành lẫn tỏi hỏng trong cùng một pass:

#### Mask 1 — Tỏi trắng / bình thường

| Kênh | Mặc định | Lý do |
|------|----------|-------|
| H | 0 – 179 | Tỏi trắng/kem có saturation rất thấp → H không đáng tin, bỏ qua ràng buộc |
| S | 0 – 60 | Chỉ bắt màu trắng/kem ít bão hòa |
| V | 170 – 255 | Tỏi sáng, loại bóng tối |

#### Mask 2 — Tỏi hỏng / nâu / tối

| Kênh | Mặc định | Lý do |
|------|----------|-------|
| H | 5 – 25 | Vàng → nâu nhạt (tông hỏng điển hình) |
| S | 40 – 255 | Đủ bão hòa — tránh nhầm với trắng |
| V | 50 – 175 | Tối hơn tỏi lành nhưng không phải nền đen |

```
mask = BitwiseOr(maskNormal, maskDamaged)
```

#### Morphology (hậu xử lý mask)

```
mask
 → MorphOpen  (Ellipse 7×7, 2 lần)  — loại nhiễu nhỏ, đốm lạ
 → MorphClose (Ellipse 7×7, 3 lần)  — lấp đầy lỗ hổng, nối vùng gần nhau
```

#### Tìm contour & lọc

- `FindContours` (External, ApproxNone — giữ **toàn bộ điểm biên** để tính chu vi chính xác)
- Loại contour có `Area < MinContourArea` (mặc định 500 px²)

#### Tính circularity kết hợp

Mỗi contour được tính **2 chỉ số độc lập** rồi lấy trung bình đều:

| Chỉ số | Công thức | Điểm mạnh | Điểm yếu |
|--------|-----------|-----------|----------|
| Isoperimetric | `4π·A / P²` | Đo độ mượt của **biên** | Oval dài mượt vẫn cho giá trị cao |
| MinEnclosingCircle | `A / (π·r²)` | Không phụ thuộc chu vi, ổn định khi biên nhiễu | Hình oval dài bị kéo thấp |

```
circularity = (isoCircularity + enclosingRatio) / 2
```

Vùng có `circularity < MinCircularity` (mặc định 0.2) bị lọc bỏ trước khi gán nhãn.

---

### Bước 3 — Crop ROI

Với mỗi `GarlicSegmentResult.BoundingRect`, crop vùng tỏi từ **frame BGR gốc** (không phải frame HSV):

```csharp
using var roi = new Mat(bgrFrame, new Rect(rx, ry, rw, rh));
```

Tọa độ được `Math.Clamp` để tránh tràn biên ảnh.

---

### Bước 4 — Trích xuất đặc trưng (`GarlicFeatureExtractor`)

Mỗi ROI được resize về **64×64** trước khi trích xuất. Tổng **~97 chiều**, toàn bộ được **min-max normalize về [0, 1]** trước khi đưa vào SVM.

#### Nhóm 1 — Đặc trưng màu sắc (~20 chiều)

| # | Đặc trưng | Cách tính | Normalize |
|---|-----------|-----------|-----------|
| 1–3 | HSV mean (H, S, V) | `MeanStdDev` trên ảnh HSV | H÷180, S÷255, V÷255 |
| 4–6 | HSV std (H, S, V) | `MeanStdDev` | H÷180, S÷255, V÷255 |
| 7 | Tỉ lệ pixel xanh lá | `InRange(H=35–85, S=40–255, V=40–255)` → CountNonZero / tổng pixel | — |
| 8 | Tỉ lệ pixel tím | `InRange(H=100–160, ...)` | — |
| 9 | Tỉ lệ pixel tối/nâu | `InRange(V=0–60)` | — |
| 10–12 | LAB mean (L, A, B) | `MeanStdDev` trên ảnh LAB | ÷255 |
| 13–15 | LAB std (L, A, B) | `MeanStdDev` | ÷255 |
| 16–23 | HSV-H histogram | `CalcHist` 8 bins, range 0–180, normalized MinMax | — |

> Các tỉ lệ pixel bất thường (xanh, tím, tối) là dấu hiệu trực tiếp nhất của tỏi hỏng.

#### Nhóm 2 — Đặc trưng kết cấu LBP (~59 chiều)

**Uniform Local Binary Pattern** trên ảnh grayscale:

```
Với mỗi pixel (y, x):
  code = 8-bit: mỗi bit = 1 nếu neighbor >= center, 0 nếu ngược lại
  (8 neighbors theo chiều kim đồng hồ, bắt đầu từ góc trên-trái)

Uniform pattern: số lần chuyển bit (0→1 hoặc 1→0) ≤ 2
  → bin = số bit 1 trong code (0–8)   [9 bins uniform]
Non-uniform: transitions > 2
  → bin = 58                           [1 bin gộp]
```

Histogram 59 bins được normalize theo tổng số pixel. LBP phân biệt được tỏi mịn (bình thường) vs tỏi có vết lồi lõm, đốm nâu (hỏng).

#### Nhóm 3 — Đặc trưng hình dạng (~6 chiều)

Lấy từ contour lớn nhất trong ảnh Otsu-threshold của ROI grayscale:

| # | Đặc trưng | Công thức | Normalize |
|---|-----------|-----------|-----------|
| 1 | Area | `ContourArea(c)` | ÷ 64² |
| 2 | Circularity | `4π·A / P²` | — (∈ [0,1]) |
| 3 | Convexity | `Area / ConvexHullArea` | — (∈ [0,1]) |
| 4 | Aspect ratio | `bbox.Width / bbox.Height` | — |
| 5 | Perimeter | `ArcLength(c)` | ÷ (4 × 64) |
| 6 | Số điểm contour | `c.Length` | ÷ (4 × 64) |

> Tỏi hỏng thường có convexity thấp (biên lõm, méo), circularity thấp (mất hình cầu).

#### Nhóm 4 — Đặc trưng thống kê (~12 chiều)

| # | Đặc trưng | Mô tả |
|---|-----------|-------|
| 1–9 | BGR mean, std, entropy | Mỗi kênh B/G/R: mean÷255, std÷255, entropy từ histogram 16 bins / log(16) |
| 10 | Gradient mean | `Sobel(X) + Sobel(Y)` → Magnitude → mean÷255 |
| 11 | Gradient std | std÷255 |
| 12 | Gradient SNR | mean / (std + ε) — đo độ sắc nét / vết hỏng rõ nét |

> SNR gradient cao → biên sắc nét (tỏi lành); SNR thấp → bề mặt nhòa, vết nâu lan rộng (tỏi hỏng).

---

### Bước 5 — Phân loại SVM Stage 1 (`SvmClassifier`)

#### Cấu hình SVM

| Tham số | Giá trị | Ý nghĩa |
|---------|---------|---------|
| Type | `C-SVC` | Support Vector Classification — phân loại đa lớp (ở đây 2 lớp Stage 1) |
| Kernel | `RBF` | Radial Basis Function: `K(x,y) = exp(-γ‖x−y‖²)` — phi tuyến, phù hợp dữ liệu không phân tách tuyến tính |
| `C` | Cấu hình được | Hệ số phạt: C lớn → ít bỏ sót nhưng dễ overfit; C nhỏ → margin rộng hơn |
| `γ` (Gamma) | Cấu hình được | Bán kính ảnh hưởng: γ lớn → mỗi điểm dữ liệu ảnh hưởng hẹp hơn |
| TermCriteria | MaxIter=1000, ε=1e-6 | Điều kiện dừng huấn luyện |

#### Nhãn SVM (Stage 1 — nội bộ)

| SVM Label | Ý nghĩa |
|-----------|---------|
| `0` | Tỏi bình thường → tiếp tục Stage 2 |
| `1` | Tỏi hỏng → gán `GarlicLabel.ToHong = 2`, kết thúc |

#### Suy luận nhãn từ thư mục dữ liệu khi train

Tên thư mục chứa ảnh xác định nhãn SVM:
- Thư mục chứa `hong` / `hu`/ `defect` → label `1` (hỏng)
- Các thư mục khác (`binh_thuong`, `to`, `nho`, `normal`...) → label `0` (bình thường)

#### Lưu / nạp model

Model được serialize sang file XML (`garlic_svm.xml`) bằng `SVM.Save()` / `SVM.Load()`.

---

### Bước 6 — Phân kích thước Stage 2

```
if (svmLabel == 0)  // bình thường
{
    if (area >= SizeThresholdPx)  // mặc định 5000 px²
        finalLabel = GarlicLabel.ToTo   // = 0
    else
        finalLabel = GarlicLabel.ToNho  // = 1
}
else
{
    finalLabel = GarlicLabel.ToHong     // = 2
}
```

`SizeThresholdPx` được lưu trong `settings.json` và cấu hình được từ UI.

---

## 6. Huấn luyện SVM

### Chuẩn bị dữ liệu

Tổ chức thư mục theo nhãn:

```
training_data/
├── binh_thuong/    # → SVM label 0 (bình thường)
│   ├── img_001.jpg
│   └── ...
├── toi_to/         # → SVM label 0 (bình thường, to)
├── toi_nho/        # → SVM label 0 (bình thường, nhỏ)
└── toi_hong/       # → SVM label 1 (hỏng)
    ├── img_001.jpg
    └── ...
```

> SVM Stage 1 chỉ phân biệt **bình thường vs hỏng** (2 lớp). `toi_to` và `toi_nho` đều được gán label `0`.

### Quy trình train trong `SvmClassifier.TrainAsync()`

1. Quét đệ quy `.jpg`, `.png`, `.bmp` trong thư mục dữ liệu
2. Với mỗi ảnh: tiền xử lý → phân vùng (`HsvSegmenter`) → trích ROI từng củ → trích đặc trưng
3. Nếu phân vùng không tìm được vùng nào → fallback dùng toàn ảnh
4. Xây dựng training matrix `float[N × 97]` và label vector `int[N]`
5. Gọi `SVM.Train()` với tham số C, Gamma được chỉ định
6. Lưu model ra `garlic_svm.xml`

### Lưu ý chọn C và Gamma (Grid Search khuyến nghị)

| C | Gamma | Hiệu ứng |
|---|-------|---------|
| Nhỏ (0.1–1) | Nhỏ (0.001–0.01) | Boundary đơn giản, underfit nếu dữ liệu phức tạp |
| Lớn (10–1000) | Trung bình (0.1–1) | Boundary phức tạp hơn, dễ overfit nếu ít dữ liệu |
| **1–100** | **0.01–0.1** | Vùng khởi đầu tốt cho bài toán này |

Nên thực hiện **cross-validation 5-fold** để chọn siêu tham số tối ưu trước khi train toàn bộ tập.

---

## 7. Pipeline camera thời gian thực (`GarlicPipeline`)

```
CameraService (~30 fps)
        │
        ├─► FrameReady (UI thread) ──────────────────► picCamera cập nhật ngay
        │       kèm CachedRegions (kết quả lần trước)
        │
        └─► pendingFrame ──► RunSegmentationLoopAsync()
                                    │  (thread-pool, chỉ 1 worker tại một thời điểm)
                                    │  Interlocked: 0=rảnh, 1=đang xử lý
                                    ▼
                          Lấy latestRawFrame (frame MỚI NHẤT)
                                    │
                                    ▼
                          Preprocess → Segment → Classify
                                    │
                                    ▼
                          SegmentationCompleted ──────────────► UI overlay kết quả
```

**Nguyên tắc thiết kế:**
- UI **không bao giờ bị block** — hiển thị mỗi frame ngay lập tức
- Phân vùng luôn lấy **frame mới nhất** (bỏ qua frame cũ nếu worker đang bận)
- `AutoSegment = false` → tắt phân vùng tự động, chỉ cập nhật live feed (dùng khi chụp ảnh thủ công)

---

## 8. Kiến trúc SOLID

| Nguyên tắc | Áp dụng |
|------------|---------|
| **SRP** | Mỗi class 1 trách nhiệm: `HsvGarlicPreprocessor` chỉ tiền xử lý, `HsvSegmenter` chỉ phân vùng, `GarlicFeatureExtractor` chỉ trích đặc trưng, `SvmClassifier` chỉ train/predict |
| **OCP** | Interface `IFeatureExtractor`, `IGarlicSegmentor`, `IImagePreprocessor`, `IGarlicClassifier` — thêm thuật toán mới không sửa code cũ |
| **LSP** | Mọi implementation thay thế được interface tương ứng |
| **ISP** | Interface nhỏ, đơn nhiệm — `ITrainer` tách khỏi `IPredictor` |
| **DIP** | `SvmClassifier` nhận `IFeatureExtractor` qua constructor injection |

---

## 9. Cài đặt và chạy

### Yêu cầu

- Windows 10/11 (64-bit)
- .NET 10 SDK
- Visual Studio 2022/2026 hoặc `dotnet run`
- Webcam (tùy chọn — có thể dùng ảnh tĩnh)

### Build & Run

```bash
cd Haui.GarlicDetector
dotnet run
```

### Cấu hình (`settings.json`)

File được copy sang thư mục output khi build (`PreserveNewest`). Các tham số quan trọng:

| Key | Mặc định | Mô tả |
|-----|----------|-------|
| `SizeThresholdPx` | `5000` | Ngưỡng diện tích (px²) phân biệt tỏi to/nhỏ |
| `MinContourArea` | `500` | Diện tích tối thiểu để coi là củ tỏi |
| `MinCircularity` | `0.2` | Ngưỡng circularity tối thiểu (lọc vật thể không phải tỏi) |
| `Hsv` | H:0–179, S:0–60, V:170–255 | Ngưỡng HSV tỏi bình thường |
| `HsvDamaged` | H:5–25, S:40–255, V:50–175 | Ngưỡng HSV tỏi hỏng |
| `DetectionRegion` | `null` | Vùng nhận diện (null = toàn frame) |

---

## 10. Ưu điểm & Hạn chế

### Ưu điểm

- **Không cần GPU** — SVM chạy hoàn toàn trên CPU
- **Dữ liệu nhỏ** — SVM hiệu quả với vài trăm đến vài nghìn mẫu
- **Huấn luyện nhanh** — vài giây đến vài phút tùy kích thước tập dữ liệu
- **Tốc độ inference cao** — phù hợp real-time ~30 fps
- **Có thể giải thích** — support vectors cho biết mẫu nào quyết định boundary
- **Thiết kế mở rộng** — thêm feature extractor mới không sửa SVM

### Hạn chế

- Kết quả phân vùng phụ thuộc vào **điều kiện ánh sáng** — cần môi trường kiểm soát tốt
- SVM với RBF cần **grid search C và Gamma** — chọn sai siêu tham số ảnh hưởng lớn đến độ chính xác
- **Không tự thích nghi** với điều kiện thay đổi — cần retrain khi môi trường chụp thay đổi đáng kể
- Với dữ liệu rất lớn (>100k mẫu) và nhiều biến thể, **deep learning** (CNN) sẽ vượt trội hơn
