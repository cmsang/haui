#!/usr/bin/env bash
# Smoke-test: quick train (few epochs) + export ONNX to verify the pipeline on Ubuntu.
# Defaults to a single model, 1 epoch. Override via env or extra args.
#
#   ./test_train.sh                       # 1 epoch, yolov8m.pt only
#   EPOCHS=2 MODEL=yolo26m.pt ./test_train.sh
#   ./test_train.sh --device 0            # extra args passed straight to train.py
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

VENV_PY="$ROOT/.venv/bin/python"

if [ ! -x "$VENV_PY" ]; then
    echo "[test_train.sh] Khong tim thay .venv. Chay setup_venv.sh truoc:"
    echo "    bash setup_venv.sh"
    exit 1
fi

EPOCHS="${EPOCHS:-1}"
BATCH="${BATCH:-1}"
MODEL="${MODEL:-yolov8m.pt}"

echo "[test_train.sh] Patch data.yaml ..."
"$VENV_PY" patch_data_yaml.py

echo "[test_train.sh] Detect hardware ..."
"$VENV_PY" detect_hardware.py

echo "[test_train.sh] Test train: model=$MODEL, epochs=$EPOCHS, batch=$BATCH ..."
"$VENV_PY" train.py --models "$MODEL" --epochs "$EPOCHS" --batch "$BATCH" "$@"

echo "[test_train.sh] Test export ONNX ($MODEL) ..."
"$VENV_PY" export_onnx.py --models "$MODEL"

echo "[test_train.sh] Hoan tat test. ONNX nam trong runs/pcb/onnx/"




