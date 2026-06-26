"""Ensure yolo_dataset/data.yaml path points to the local dataset folder."""
from __future__ import annotations

import argparse
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent
DEFAULT_DATA_YAML = ROOT / "yolo_dataset" / "data.yaml"
DATASET_ROOT = ROOT / "yolo_dataset"


def patch(data_yaml: Path = DEFAULT_DATA_YAML) -> Path:
    if not data_yaml.exists():
        raise FileNotFoundError(f"data.yaml not found: {data_yaml}")

    with open(data_yaml, "r", encoding="utf-8") as f:
        doc = yaml.safe_load(f) or {}

    # Ultralytics expects forward slashes in path
    absolute_path = DATASET_ROOT.resolve().as_posix()
    doc["path"] = absolute_path

    # Preserve train/val relative paths if missing
    doc.setdefault("train", "images/train")
    doc.setdefault("val", "images/val")

    with open(data_yaml, "w", encoding="utf-8") as f:
        yaml.safe_dump(doc, f, default_flow_style=False, allow_unicode=True, sort_keys=False)

    print(f"Patched {data_yaml}")
    print(f"  path: {absolute_path}")
    return data_yaml


def main() -> None:
    parser = argparse.ArgumentParser(description="Patch local data.yaml path.")
    parser.add_argument(
        "--data-yaml",
        type=Path,
        default=DEFAULT_DATA_YAML,
        help="Path to data.yaml",
    )
    args = parser.parse_args()
    patch(args.data_yaml)


if __name__ == "__main__":
    main()
