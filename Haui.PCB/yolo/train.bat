@echo off
REM Train YOLOv8m + YOLO26m for PCB detection (960x1280), then auto-export ONNX.
setlocal
cd /d "%~dp0"

set "VENV_PY=.venv\Scripts\python.exe"

if not exist "%VENV_PY%" (
    echo [train.bat] Khong tim thay .venv. Chay setup_venv.ps1 truoc:
    echo     powershell -ExecutionPolicy Bypass -File setup_venv.ps1
    exit /b 1
)

echo [train.bat] Patch data.yaml ...
"%VENV_PY%" patch_data_yaml.py
if errorlevel 1 goto :error

echo [train.bat] Detect hardware ...
"%VENV_PY%" detect_hardware.py
if errorlevel 1 goto :error

echo [train.bat] Bat dau train (YOLOv8m + YOLO26m) ...
"%VENV_PY%" train.py %*
if errorlevel 1 goto :error

echo [train.bat] Export ONNX (ten phan biet tung model) ...
"%VENV_PY%" export_onnx.py
if errorlevel 1 goto :error

echo [train.bat] Hoan tat. ONNX nam trong runs\pcb\onnx\
exit /b 0

:error
echo [train.bat] Loi khi chay. Xem log phia tren.
exit /b 1
