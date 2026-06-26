#Requires -Version 5.1
<#
.SYNOPSIS
    Create virtual environment and install dependencies for YOLOv8 training.
#>
$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
Set-Location $Root

Write-Host "=== YOLO train environment setup ===" -ForegroundColor Cyan

if (-not (Test-Path ".venv")) {
    Write-Host "Creating .venv ..."
    python -m venv .venv
} else {
    Write-Host ".venv already exists, reusing."
}

$Activate = Join-Path $Root ".venv\Scripts\Activate.ps1"
if (-not (Test-Path $Activate)) {
    throw "venv activation script not found: $Activate"
}

. $Activate

python -m pip install --upgrade pip

# Try NVIDIA CUDA wheel first; pip falls back if index unavailable
Write-Host "Installing PyTorch ..."
$torchOk = $false
try {
    pip install torch torchvision --index-url https://download.pytorch.org/whl/cu124
    $torchOk = $true
} catch {
    Write-Host "CUDA wheel install failed, trying default PyPI CPU build ..." -ForegroundColor Yellow
}

if (-not $torchOk) {
    pip install torch torchvision
}

Write-Host "Installing requirements.txt ..."
pip install -r requirements.txt

Write-Host "Detecting hardware ..."
python detect_hardware.py

Write-Host "Patching data.yaml ..."
python patch_data_yaml.py

Write-Host ""
Write-Host "Done. Activate with:" -ForegroundColor Green
Write-Host "  .\.venv\Scripts\Activate.ps1"
Write-Host "Then train:"
Write-Host "  python train.py"
