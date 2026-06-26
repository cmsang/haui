#!/usr/bin/env bash
# Train YOLOv8m + YOLO26m for PCB detection (960x1280), then auto-export ONNX (Ubuntu).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

VENV_PY="$ROOT/.venv/bin/python"

if [ ! -x "$VENV_PY" ]; then
    echo "[train.sh] Khong tim thay .venv. Chay setup_venv.sh truoc:"
    echo "    bash setup_venv.sh"
    exit 1
fi

echo "[train.sh] Patch data.yaml ..."
"$VENV_PY" patch_data_yaml.py

echo "[train.sh] Detect hardware ..."
"$VENV_PY" detect_hardware.py

echo "[train.sh] Bat dau train (YOLOv8m + YOLO26m) ..."
"$VENV_PY" train.py "$@"

echo "[train.sh] Export ONNX (ten phan biet tung model) ..."
"$VENV_PY" export_onnx.py

echo "[train.sh] Hoan tat. ONNX nam trong runs/pcb/onnx/"
