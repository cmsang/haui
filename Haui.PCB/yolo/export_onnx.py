"""Export trained YOLO weights to ONNX (fixed 1280x960 portrait).

Exports each configured model and copies the result to a clearly named file:
    runs/pcb/onnx/{stem}_960x1280.onnx
so YOLOv8 and YOLO26 outputs are easy to distinguish.
"""
from __future__ import annotations

import argparse
import shutil
from pathlib import Path

from ultralytics import YOLO

from common import (
    ROOT,
    export_imgsz,
    load_config,
    resolve_model_list,
    run_name,
)


def default_weights(cfg: dict, model_name: str) -> Path:
    project = cfg.get("project", "runs/pcb")
    name = run_name(cfg, model_name)
    return ROOT / project / name / "weights" / "best.pt"


def onnx_output_dir(cfg: dict) -> Path:
    project = cfg.get("project", "runs/pcb")
    subdir = cfg.get("onnx_output_subdir", "onnx")
    out = ROOT / project / subdir
    out.mkdir(parents=True, exist_ok=True)
    return out


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Export YOLO weights to ONNX.")
    p.add_argument("--weights", type=Path, help="Export a single best.pt instead of config models")
    p.add_argument("--name", help="Output name (without extension) when using --weights")
    p.add_argument("--device", help="Export device (cpu or 0)")
    p.add_argument("--models", nargs="+", help="Override model list to export")
    return p.parse_args()


def export_one(weights: Path, out_name: str, cfg: dict, device) -> Path | None:
    if not weights.exists():
        print(f"!!! Weights not found, skip: {weights}")
        return None

    imgsz = export_imgsz(cfg)
    opset = int(cfg.get("onnx_opset", 12))
    simplify = bool(cfg.get("onnx_simplify", True))
    dynamic = bool(cfg.get("onnx_dynamic", False))

    print(f"\n=== Exporting {weights} ===")
    print(f"imgsz(H,W)={imgsz}, opset={opset}, simplify={simplify}, dynamic={dynamic}")

    model = YOLO(str(weights))
    onnx_path = Path(
        model.export(
            format="onnx",
            imgsz=imgsz,
            opset=opset,
            simplify=simplify,
            dynamic=dynamic,
            device=device,
        )
    )

    target = onnx_output_dir(cfg) / f"{out_name}.onnx"
    shutil.copy2(onnx_path, target)
    print(f"--- ONNX: {target}")
    return target


def main() -> None:
    args = parse_args()
    cfg = load_config()
    device = args.device if args.device is not None else cfg.get("device", "cpu")

    exported: list[Path] = []

    if args.weights:
        out_name = args.name or args.weights.parent.parent.name or args.weights.stem
        result = export_one(args.weights, out_name, cfg, device)
        if result:
            exported.append(result)
    else:
        models = args.models or resolve_model_list(cfg)
        for model_name in models:
            weights = default_weights(cfg, model_name)
            out_name = run_name(cfg, model_name)
            result = export_one(weights, out_name, cfg, device)
            if result:
                exported.append(result)

    print("\n==== Export summary ====")
    if not exported:
        print("  Khong co ONNX nao duoc tao. Train truoc: python train.py")
    for path in exported:
        print(f"  {path}")


if __name__ == "__main__":
    main()
