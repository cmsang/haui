# Train YOLOv8m + YOLO26m — PCB component detection

Project train **nhiều mô hình** (mặc định **YOLOv8m** và **YOLO26m**) trên dataset xuất từ app Haui.PCB (`yolo_dataset/`), độ phân giải mục tiêu **960×1280** (portrait), export **ONNX** với tên phân biệt từng model sau khi train.

## Yêu cầu

- Windows x64
- Python **3.11–3.12** khuyến nghị (3.14 có thể chưa có wheel PyTorch)
- GPU NVIDIA + CUDA (khuyến nghị cho `imgsz=1280`); không có GPU vẫn train được trên CPU (rất chậm)

## Cài đặt

```powershell
cd D:\haui\Haui.PCB\yolo
.\setup_venv.ps1
.\.venv\Scripts\Activate.ps1
```

`setup_venv.ps1` sẽ:

1. Tạo `.venv`
2. Cài PyTorch (CUDA nếu phát hiện GPU, ngược lại CPU)
3. Cài `requirements.txt`
4. Chạy `detect_hardware.py` → sinh `config.local.yaml` (batch, workers, device, cache)
5. Chạy `patch_data_yaml.py` → sửa `path` trong `yolo_dataset/data.yaml`

## Chọn model train

Sửa `models` trong [`config.yaml`](config.yaml):

```yaml
models:
  - "yolov8m.pt"
  - "yolo26m.pt"
```

Mỗi model train vào thư mục run riêng: `runs/pcb/{stem}_960x1280/` (vd `yolov8m_960x1280`, `yolo26m_960x1280`).

## Train

Cách nhanh — chạy file batch (tự patch data + detect hardware + train **tất cả model** + export ONNX):

```powershell
.\train.bat
```

Hoặc chạy trực tiếp trong venv:

```powershell
python train.py
# chỉ train một số model
python train.py --models yolov8m.pt
# override nhanh
python train.py --epochs 50 --batch 2
```

Kết quả mỗi model: `runs/pcb/{stem}_960x1280/weights/best.pt`

## Export ONNX

```powershell
.\export_onnx.bat
# hoặc trong venv:
python export_onnx.py
# chỉ định weights đơn lẻ
python export_onnx.py --weights runs/pcb/yolo26m_960x1280/weights/best.pt --name yolo26m_960x1280
```

ONNX của tất cả model được gom vào **`runs/pcb/onnx/`** với tên phân biệt:

```
runs/pcb/onnx/yolov8m_960x1280.onnx
runs/pcb/onnx/yolo26m_960x1280.onnx
```

Input cố định **1280×960** (H×W).

## Độ phân giải 960×1280

| Giai đoạn | Cấu hình |
|-----------|----------|
| Train / val | `imgsz=1280`, `rect=true` — cạnh dài 1280, giữ tỉ lệ 3:4 |
| Export / infer | `imgsz=(1280, 960)` — portrait cố định |

## Tuning CPU + GPU

- **GPU**: forward/backward + `amp=true`
- **CPU**: DataLoader (`workers`), augmentation, `cache=ram|disk`
- Chạy lại `python detect_hardware.py` sau khi đổi phần cứng

### GPU AMD/Intel (tùy chọn)

Có thể thử [torch-directml](https://github.com/microsoft/DirectML) — chưa bật mặc định; cần cài thủ công và đặt `device` trong `config.local.yaml`.

## Cấu trúc

```
yolo/
├── config.yaml           # Tham số mặc định (danh sách models)
├── config.local.yaml     # Sinh tự động — không commit
├── train.bat             # Chạy nhanh: patch + train tất cả model + export ONNX
├── export_onnx.bat       # Chạy nhanh: export ONNX
├── common.py             # Load config + đặt tên run/ONNX dùng chung
├── train.py
├── export_onnx.py
├── detect_hardware.py
├── patch_data_yaml.py
├── setup_venv.ps1
└── yolo_dataset/         # images/, labels/, data.yaml
```

## Lỗi thường gặp

| Lỗi | Cách xử lý |
|-----|------------|
| `No module named 'torch'` | Chạy lại `setup_venv.ps1`, kích hoạt `.venv` |
| PyTorch không cài trên Python 3.14 | Cài Python 3.12, tạo venv: `py -3.12 -m venv .venv` |
| CUDA out of memory | Giảm `batch` trong `config.local.yaml` (1 hoặc 2) |
| Train quá chậm | Dùng GPU NVIDIA, bớt model trong `models`, hoặc thử `yolov8s.pt` |
| CUDA out of memory @ 1280 | Giảm `batch` hoặc giảm `imgsz` về 1024 trong `config.yaml` |
| YOLO26 lỗi cài trên Python 3.14 | Dùng Python 3.12; hoặc bỏ `yolo26m.pt` khỏi `models` |
