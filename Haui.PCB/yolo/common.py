"""Shared config loading and naming helpers for the YOLO train project."""
from __future__ import annotations

from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent
CONFIG_PATH = ROOT / "config.yaml"
LOCAL_CONFIG_PATH = ROOT / "config.local.yaml"


def load_config() -> dict:
    """Load config.yaml and merge optional config.local.yaml overrides."""
    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        cfg = yaml.safe_load(f) or {}

    if LOCAL_CONFIG_PATH.exists():
        with open(LOCAL_CONFIG_PATH, "r", encoding="utf-8") as f:
            local = yaml.safe_load(f) or {}
        cfg.update(local)

    return cfg


def resolve_model_list(cfg: dict) -> list[str]:
    """Return configured models, supporting both `models` list and legacy `model`."""
    models = cfg.get("models")
    if isinstance(models, (list, tuple)) and models:
        return [str(m) for m in models]

    single = cfg.get("model")
    if single:
        return [str(single)]

    return ["yolov8m.pt"]


def model_stem(model_name: str) -> str:
    """Filename without extension, e.g. 'yolo26m.pt' -> 'yolo26m'."""
    return Path(model_name).stem


def run_name(cfg: dict, model_name: str) -> str:
    """Build the run/output name for a model from name_template."""
    template = cfg.get("name_template", "{stem}_960x1280")
    return template.format(stem=model_stem(model_name))


def resolve_data_path(cfg: dict) -> str:
    data = cfg.get("data", "yolo_dataset/data.yaml")
    path = Path(data)
    if not path.is_absolute():
        path = ROOT / path
    return str(path.resolve())


def export_imgsz(cfg: dict) -> tuple[int, int]:
    value = cfg.get("export_imgsz", [1280, 960])
    if isinstance(value, (list, tuple)) and len(value) == 2:
        return int(value[0]), int(value[1])
    return 1280, 960
