# ONNX models

Place trained models here for runtime inspection, e.g.:

- `yolo26m_960x1280.onnx` — default path in `Config/setting.json` → `ComponentDetection.modelPath`

Copy from YOLO export output:

```
yolo/runs/pcb/onnx/yolo26m_960x1280.onnx
```

Files in this folder are copied to the app output directory when present (see `Haui.PCB.csproj`).
