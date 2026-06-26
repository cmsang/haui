"""Train one or more YOLO models (YOLOv8 / YOLO26) on the PCB dataset.

Each configured model trains into its own run folder named "{stem}_960x1280"
(960x1280 portrait via rect training).
"""
from __future__ import annotations

import argparse
from pathlib import Path

from ultralytics import YOLO

from common import (
    ROOT,
    load_config,
    resolve_data_path,
    resolve_model_list,
    run_name,
)
from patch_data_yaml import patch as patch_data_yaml


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Train YOLO models for PCB detection.")
    p.add_argument("--epochs", type=int)
    p.add_argument("--batch", type=int)
    p.add_argument("--imgsz", type=int)
    p.add_argument("--device")
    p.add_argument("--workers", type=int)
    p.add_argument(
        "--models",
        nargs="+",
        help="Override model list, e.g. --models yolov8m.pt yolo26m.pt",
    )
    return p.parse_args()


def train_one(model_name: str, cfg: dict, data_path: str, args: argparse.Namespace) -> Path:
    imgsz = args.imgsz or cfg.get("imgsz", 1280)
    epochs = args.epochs or cfg.get("epochs", 100)
    batch = args.batch if args.batch is not None else cfg.get("batch", 1)
    device = args.device if args.device is not None else cfg.get("device", "cpu")
    workers = args.workers if args.workers is not None else cfg.get("workers", 4)
    rect = bool(cfg.get("rect", True))
    patience = cfg.get("patience", 20)
    project = cfg.get("project", "runs/pcb")
    name = run_name(cfg, model_name)
    amp = bool(cfg.get("amp", False))
    cache = cfg.get("cache", "disk")

    print(f"\n=== Training {model_name} -> run '{name}' ===")
    print(f"imgsz={imgsz}, rect={rect}, batch={batch}, device={device}, workers={workers}")
    print(f"amp={amp}, cache={cache}, epochs={epochs}")

    model = YOLO(model_name)
    results = model.train(
        data=data_path,
        epochs=epochs,
        imgsz=imgsz,
        batch=batch,
        device=device,
        workers=workers,
        rect=rect,
        patience=patience,
        project=str(ROOT / project),
        name=name,
        exist_ok=True,
        amp=amp,
        cache=cache,
    )

    best = Path(results.save_dir) / "weights" / "best.pt"
    print(f"--- {model_name} done. Best weights: {best}")
    return best


def main() -> None:
    args = parse_args()
    cfg = load_config()

    patch_data_yaml()

    data_path = resolve_data_path(cfg)
    if not Path(data_path).exists():
        raise FileNotFoundError(f"Dataset config not found: {data_path}")

    models = args.models or resolve_model_list(cfg)
    print(f"Data:   {data_path}")
    print(f"Models: {', '.join(models)}")

    trained: list[tuple[str, Path]] = []
    for model_name in models:
        try:
            best = train_one(model_name, cfg, data_path, args)
            trained.append((model_name, best))
        except Exception as ex:  # keep training remaining models on failure
            print(f"!!! Training failed for {model_name}: {ex}")

    print("\n==== Training summary ====")
    for model_name, best in trained:
        print(f"  {model_name}: {best}")
    if trained:
        print("\nExport all to ONNX: python export_onnx.py")


if __name__ == "__main__":
    main()
