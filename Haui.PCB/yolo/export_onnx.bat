@echo off
REM Export trained YOLOv8m weights to ONNX (1280x960).
setlocal
cd /d "%~dp0"

set "VENV_PY=.venv\Scripts\python.exe"

if not exist "%VENV_PY%" (
    echo [export_onnx.bat] Khong tim thay .venv. Chay setup_venv.ps1 truoc.
    exit /b 1
)

"%VENV_PY%" export_onnx.py %*
if errorlevel 1 (
    echo [export_onnx.bat] Loi khi export.
    exit /b 1
)
exit /b 0
