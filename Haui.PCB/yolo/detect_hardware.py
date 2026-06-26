"""Probe CPU/RAM/GPU and write config.local.yaml for YOLO training."""
from __future__ import annotations

import os
from pathlib import Path

import psutil
import yaml

ROOT = Path(__file__).resolve().parent
OUTPUT_PATH = ROOT / "config.local.yaml"


def _suggest_batch(vram_gb: float | None, imgsz: int = 1280) -> int:
    """Heuristic batch size for YOLOv8m at the given image size."""
    if vram_gb is None:
        return 1

    # YOLOv8m @ imgsz=1280 is VRAM-heavy
    if vram_gb >= 20:
        return 12
    if vram_gb >= 12:
        return 6
    if vram_gb >= 8:
        return 3
    return 1


def _suggest_cache(total_ram_gb: float, image_count_estimate: int = 1500) -> str:
    """Prefer RAM cache when enough free memory for ~1.5k medium images."""
    free_gb = psutil.virtual_memory().available / (1024**3)
    if free_gb >= 16 and total_ram_gb >= 32:
        return "ram"
    if free_gb >= 8:
        return "disk"
    return "disk"


def detect() -> dict:
    cpu_count = os.cpu_count() or 4
    workers = max(1, min(8, cpu_count - 1))

    total_ram_gb = psutil.virtual_memory().total / (1024**3)
    cache = _suggest_cache(total_ram_gb)

    device = "cpu"
    amp = False
    batch = 1
    notes: list[str] = []

    try:
        import torch

        if torch.cuda.is_available():
            device = 0
            amp = True
            props = torch.cuda.get_device_properties(0)
            vram_gb = props.total_memory / (1024**3)
            batch = _suggest_batch(vram_gb)
            notes.append(f"CUDA GPU: {props.name}, VRAM {vram_gb:.1f} GB")
        else:
            notes.append("CUDA not available — training on CPU")
            batch = 1
            amp = False
    except ImportError:
        notes.append("torch not installed — run setup_venv.ps1 first")

    return {
        "device": device,
        "batch": batch,
        "workers": workers,
        "amp": amp,
        "cache": cache,
        "_detected": {
            "cpu_logical": cpu_count,
            "ram_gb": round(total_ram_gb, 1),
            "notes": notes,
        },
    }


def main() -> None:
    cfg = detect()
    detected = cfg.pop("_detected", {})

    with open(OUTPUT_PATH, "w", encoding="utf-8") as f:
        yaml.safe_dump(cfg, f, default_flow_style=False, allow_unicode=True, sort_keys=False)

    print(f"Wrote {OUTPUT_PATH}")
    for note in detected.get("notes", []):
        print(f"  {note}")
    print(f"  device={cfg['device']}, batch={cfg['batch']}, workers={cfg['workers']}, "
          f"amp={cfg['amp']}, cache={cfg['cache']}")


if __name__ == "__main__":
    main()
