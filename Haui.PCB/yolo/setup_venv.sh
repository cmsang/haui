#!/usr/bin/env bash
# Create virtual environment and install dependencies for YOLO training (Ubuntu).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

# Pick a Python interpreter (prefer python3)
PY="${PYTHON:-python3}"
command -v "$PY" >/dev/null 2>&1 || PY=python

echo "=== YOLO train environment setup ==="

if [ ! -d ".venv" ]; then
    echo "Creating .venv ..."
    "$PY" -m venv .venv
else
    echo ".venv already exists, reusing."
fi

# shellcheck disable=SC1091
source .venv/bin/activate

python -m pip install --upgrade pip

# Try NVIDIA CUDA wheel first; fall back to default PyPI (CPU) build.
echo "Installing PyTorch ..."
if ! pip install torch torchvision --index-url https://download.pytorch.org/whl/cu124; then
    echo "CUDA wheel install failed, trying default PyPI build ..."
    pip install torch torchvision
fi

echo "Installing requirements.txt ..."
pip install -r requirements.txt

echo "Detecting hardware ..."
python detect_hardware.py

echo "Patching data.yaml ..."
python patch_data_yaml.py

echo ""
echo "Done. Activate with:"
echo "  source .venv/bin/activate"
echo "Then train:"
echo "  ./train.sh"
