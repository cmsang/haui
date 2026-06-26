# Product Context — Haui.PCB

## Why this exists

Operators inspect PCB boards against a visual template: capture under camera, auto-crop/straighten, then check whether defined regions match a reference image.

## Users

- Lab/line operators with a camera over a work surface
- Developers tuning segmentation and comparison

## UI language

**Vietnamese** labels and status messages (correct diacritics; `MaterialDesignFont` / `Segoe UI`). Source comments: **English only** — see `.cursor/rules/language-and-ui-text.mdc`.

## Primary workflow

1. **MainWindow** — camera, resolution (default 1280×720), live preview + FPS
2. **Optional ROI** — rectangle on preview → `last_region.json`
3. **Toolbar** (capture frame, open child window):
   - **Test** → segment, compare all library templates, best match detail (camera capture)
   - **Chọn ảnh** (DeveloperMode) → same inspection from a file on disk; result in `ResultImage` only — `CameraImage` stays live camera feed
   - **Tạo mẫu** → segment, draw regions, save template
   - **Xem mẫu** → browse library, edit/delete

## Comparison semantics

- Per-region similarity (NCC + histogram); **`IsMatch` when ≥ `MinMatchSimilarityPercent`**
- **Component library** (`TrainWhiteCircuit` off): match → **có linh kiện**; PASS when every region has a component
- **White-circuit library** (`TrainWhiteCircuit` on): match → **không có linh kiện** (empty pad); PASS when every region has a component
- UI: “Có linh kiện” / “Thiếu”; overlay green = present, red = missing

## Template storage (operator-visible)

One library folder (`templates/` or custom via settings): each sample = PNG + `*_regions.json`. Create, Viewer, and Test share the same library.
