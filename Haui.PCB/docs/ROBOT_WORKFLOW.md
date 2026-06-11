# Tài liệu luồng hoạt động Robot — Haui.PCB

Tài liệu mô tả các luồng điều khiển robot 5 khớp RRRRR + gripper trong hệ thống Pick & Place PCB, giao tiếp qua SerialPort và lưu cấu hình vị trí trên SQL Server.

---

## 1. Tổng quan hệ thống

| Thành phần | Mô tả |
|------------|--------|
| **Robot** | Cánh tay 5 DOF (J1–J5) + gripper, firmware nhận lệnh ASCII qua COM |
| **Warehouse** | Thiết bị/PLC phụ trên cổng `warehouseCom`, nhận lệnh C1/C2 khi buffer đầy |
| **SQL Server** | Bảng `RobotConfig` — lưu tọa độ teach và trạng thái slot (`EMPTY` / `FULL`) |
| **Ứng dụng** | WPF `Haui.PCB` — MainWindow, Robot Teaching, Manual Control |

### Kiến trúc phần mềm

```
┌─────────────────────────────────────────────────────────────┐
│  UI (WPF)                                                    │
│  MainWindow · RobotTeachingWindow · ManualControlWindow      │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  Processing (Haui.PCB)                                       │
│  RobotSerialService · RobotPickPlaceExecutor                 │
│  MaterialTransferService · RobotStartupHandshakeService      │
│  RobotConfigService (adapter)                                │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  BL.PCBDetect — RobotConfigBL                                │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  DL.PCBDetect — RobotConfigRepository                        │
│  Stored Procedures trên SQL Server                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Cấu hình (`Config/setting.json`)

| Khóa | Ý nghĩa | Ví dụ |
|------|---------|-------|
| `com` | Cổng COM robot | `COM20` |
| `warehouseCom` | Cổng COM warehouse | `COM22` |
| `baudRate` | Tốc độ truyền (robot + warehouse) | `115200` |
| `stepsPerDeg` | Bước motor / 1 độ | `100` |
| `jogStepDegrees` | Bước jog trên màn Teaching | `15` |
| `speedPercent` | Tốc độ Go To (0–100%) | `31` |
| `DatabaseConnection` | Chuỗi kết nối SQL Server | `SmartWarehouse` |

**Lưu ý:** Cần `TrustServerCertificate=True` nếu SQL Express dùng chứng chỉ tự ký.

---

## 3. Giao thức Serial robot

Mọi lệnh ASCII gửi qua `SendAscii(cmd)` được firmware nhận dạng **`{cmd}x`** (thêm hậu tố `x`).

| Lệnh gửi | Ý nghĩa |
|----------|---------|
| `Rx` | Handshake khởi động — hỏi robot sẵn sàng |
| `Yx` | Robot phản hồi sẵn sàng (nhận) |
| `H0x` | Homing tất cả trục (3→2→1→4→5) |
| `H1x` … `H5x` | Homing một trục |
| `Mj1,j2,j3,j4,j5x` | Di chuyển tới tọa độ 5 khớp (độ) |
| `J1+10x`, `JG-5x` | Jog từng trục / gripper |
| `G90x`, `G0x` | Mở / đóng gripper (góc độ) |
| `Dx` | Robot báo **hoàn thành** bước di chuyển (nhận) |

### Phản hồi homing

| Nhận | Ý nghĩa |
|------|---------|
| `Ax` | Bắt đầu homing trục |
| `Dx` | Hoàn thành homing / hoàn thành lệnh move |

### Lệnh warehouse (cổng `warehouseCom`)

| Lệnh | Điều kiện |
|------|-----------|
| `C1x` | Tất cả slot **OK1–OK6** có `FullState = FULL` |
| `C2x` | Tất cả slot **NG1–NG6** có `FullState = FULL` |

---

## 4. Danh sách vị trí teach chuẩn

| Nhóm | Tên vị trí | Vai trò |
|------|------------|---------|
| Chung | `PickUp` | Điểm gắp PCB |
| Chung | `Home` | Vị trí home (tham chiếu) |
| Chung | `Wait` | Điểm chờ trung gian |
| OK | `OK1` … `OK6` | Buffer hàng **Pass** |
| NG | `NG1` … `NG6` | Buffer hàng **Fail** |

Tọa độ J1–J5 lưu trong bảng `RobotConfig`. Gripper chỉ dùng trên UI/Serial, **không** lưu Database.

---

## 5. Database — bảng `RobotConfig`

### Cấu trúc

| Cột | Kiểu | Mô tả |
|-----|------|--------|
| `ID` | UNIQUEIDENTIFIER | Khóa bản ghi |
| `PosName` | NVARCHAR | Tên vị trí (PickUp, OK1, …) |
| `PosGroup` | NVARCHAR | Chung / OK / NG |
| `J1` … `J5` | NVARCHAR | Góc khớp (chuỗi số) |
| `FullState` | NVARCHAR | `EMPTY` hoặc `FULL` (slot OK/NG) |
| `UpdateTime` | DATETIME | Thời điểm cập nhật |

### Stored Procedures

| SP | Mục đích |
|----|----------|
| `Get_RobotConfig_All` | Tải toàn bộ vị trí |
| `Get_RobotConfig_ByName` | Tải một vị trí theo tên |
| `Update_RobotConfig_TeachPoint` | Cập nhật J1–J5 khi teach |
| `Update_RobotConfig_FullState` | Đổi `FullState` slot (EMPTY ↔ FULL) |

Script SQL: thư mục `Database/` (`01` → `02` → `03`).

---

## 6. Luồng khởi động (MainWindow Load)

**Màn hình:** `MainWindow` — sự kiện `Window_Loaded`  
**Service:** `RobotStartupHandshakeService`

```mermaid
sequenceDiagram
    participant App
    participant Robot

    App->>Robot: Rx
    alt Nhận Yx trong 1 giây
        Robot-->>App: Yx
        App->>Robot: H0x
        Note over App: Handshake hoàn tất
    else Chưa nhận Yx
        Note over App: Đợi 1 giây
        App->>Robot: Rx (lặp lại)
    end
```

**Chi tiết:**
1. Kết nối cổng `com` từ `setting.json`.
2. Gửi `Rx`.
3. Chờ phản hồi bắt đầu bằng `Y` (tức `Yx`).
4. Nếu chưa nhận trong **1 giây** → gửi lại `Rx`.
5. Khi nhận `Yx` → gửi `H0x` (homing toàn bộ).
6. Chỉ chạy **một lần** mỗi phiên mở app (`IsCompleted`).

---

## 7. Luồng Teach vị trí (Robot Teaching)

**Màn hình:** `RobotTeachingWindow` (hoặc tab `RobotTeachingTabView` trong MainWindow)  
**ViewModel:** `RobotTeachViewModel`

```mermaid
flowchart LR
    A[Mở Teaching] --> B[Load vị trí từ DB]
    B --> C[Jog / di chuyển robot]
    C --> D[Nhấn Teach vị trí]
    D --> E[Lưu J1-J5 qua BL/DL]
    E --> F[SP Update_RobotConfig_TeachPoint]
```

**Chi tiết:**
1. `ReloadTeachPoints()` — gọi `Get_RobotConfig_All` qua BL → DL.
2. Người dùng chọn vị trí (PickUp, OK1, …), chỉnh khớp trên UI hoặc jog qua Serial.
3. **Teach vị trí** — ghi J1–J5 của điểm đang chọn vào Database (không đổi `FullState`).
4. **Lưu cấu hình** — ghi `jogStepDegrees`, `speedPercent`, COM… vào `setting.json`.
5. Nếu Database lỗi → hiển thị thông báo trên thanh trạng thái (không fallback file JSON).

---

## 8. Luồng Manual Control

**Màn hình:** `ManualControlWindow` (hoặc tab `ManualControlTabView` trong MainWindow)  
**ViewModel:** `ManualControlViewModel`  
**Executor:** `RobotPickPlaceExecutor`

Dùng để **test thủ công** chu trình gắp–đặt tới một slot OK hoặc NG do người dùng chọn.

### Chu trình 8 bước (mỗi bước chờ `Dx`)

| Bước | Hành động |
|------|-----------|
| 1/8 | `G90x` — Mở gripper |
| 2/8 | `M…x` — Move → **PickUp** |
| 3/8 | `G0x` — Đóng gripper (gắp hàng) |
| 4/8 | `M…x` — Move → **Wait** |
| 5/8 | `M…x` — Move → **OKx / NGx** (đích đã chọn) |
| 6/8 | `G90x` — Mở gripper (thả hàng) |
| 7/8 | `M…x` — Move → **Wait** |
| 8/8 | `G0x` — Đóng gripper |

- Timeout mỗi bước: **120 giây**.
- Manual Control **không** cập nhật `FullState` trong Database.

---

## 9. Luồng Pass / Fail material (MainWindow)

**Nút:** Pass / Fail trên sidebar `MainWindow`  
**Service:** `MaterialTransferService` → `MainViewModel`

### 9.1. Chọn slot đích

| Nút | Nhóm slot | Quy tắc chọn |
|-----|-----------|--------------|
| **Pass** | OK1–OK6 | Slot **đầu tiên** có `FullState = EMPTY` |
| **Fail** | NG1–NG6 | Slot **đầu tiên** có `FullState = EMPTY` |

Nếu tất cả slot nhóm đó đã `FULL` → không chạy robot, gửi warehouse (mục 9.3) và báo lỗi.

### 9.2. Chu trình sau khi chọn slot

```mermaid
flowchart TD
    A[Nhấn Pass hoặc Fail] --> B[Load RobotConfig từ DB]
    B --> C{Tìm slot EMPTY?}
    C -->|Không| D[Gửi C1x hoặc C2x nếu buffer đầy]
    C -->|Có| E[Chạy chu trình 8 bước]
    E --> F[Cập nhật FullState = FULL]
    F --> G{Tất cả OK/NG đều FULL?}
    G -->|Có| H[Gửi C1x hoặc C2x]
    G -->|Không| I[Hoàn tất]
    H --> I
```

1. Load vị trí từ Database (PickUp, Wait, slot đích).
2. Kết nối robot COM nếu chưa online.
3. Chạy **cùng chu trình 8 bước** như Manual Control (`RobotPickPlaceExecutor`).
4. Gọi `Update_RobotConfig_FullState` → đặt slot vừa đặt hàng = **`FULL`**.
5. Nếu sau bước này **toàn bộ OK** (hoặc **toàn bộ NG**) đều `FULL` → gửi lệnh warehouse.

### 9.3. Báo warehouse buffer đầy

| Điều kiện | Lệnh gửi | Cổng |
|-----------|----------|------|
| OK1–OK6 đều `FULL` | `C1x` | `warehouseCom` |
| NG1–NG6 đều `FULL` | `C2x` | `warehouseCom` |

Gửi qua cổng riêng (`COM22`), mở COM → gửi → đóng (không dùng chung cổng robot).

---

## 10. Sơ đồ tổng hợp vận hành

```mermaid
flowchart TB
    subgraph Startup
        S1[Load MainWindow]
        S2[Connect COM robot]
        S3[Rx → Yx → H0x]
        S1 --> S2 --> S3
    end

    subgraph Teaching
        T1[RobotTeachingWindow]
        T2[Teach → Update DB J1-J5]
        T1 --> T2
    end

    subgraph Production
        P1[Phát hiện PCB]
        P2{Pass?}
        P3[Pass → slot OK EMPTY]
        P4[Fail → slot NG EMPTY]
        P5[Pick & Place 8 bước]
        P6[FullState = FULL]
        P7[Buffer đầy? → C1x/C2x]
        P1 --> P2
        P2 -->|Đạt| P3 --> P5
        P2 -->|Lỗi| P4 --> P5
        P5 --> P6 --> P7
    end

    Startup --> Teaching
    Teaching --> Production
```

---

## 11. File mã nguồn liên quan

| File / thư mục | Vai trò |
|----------------|---------|
| `Haui.PCB/Processing/RobotSerialService.cs` | Gửi/nhận COM robot |
| `Haui.PCB/Processing/RobotSerialProtocol.cs` | Định dạng lệnh M/J/H/G/R/C |
| `Haui.PCB/Processing/RobotStartupHandshakeService.cs` | Handshake Rx/Yx/H0 |
| `Haui.PCB/Processing/RobotPickPlaceExecutor.cs` | Chu trình 8 bước + chờ Dx |
| `Haui.PCB/Processing/MaterialTransferService.cs` | Pass/Fail + FULL + warehouse |
| `Haui.PCB/Processing/RobotConfigService.cs` | Adapter UI → BL |
| `BL.PCBDetect/RobotConfigBL.cs` | Nghiệp vụ RobotConfig |
| `DL.PCBDetect/RobotConfigRepository.cs` | Gọi Stored Procedure |
| `Haui.PCB/ViewModels/RobotTeachViewModel.cs` | Màn Teaching |
| `Haui.PCB/ViewModels/ManualControlViewModel.cs` | Màn Manual Control |
| `Haui.PCB/Models/RobotTeachPositions.cs` | Danh sách vị trí chuẩn |
| `Database/*.sql` | Schema, seed, stored procedures |

---

## 12. Xử lý lỗi thường gặp

| Triệu chứng | Nguyên nhân | Hướng xử lý |
|-------------|-------------|-------------|
| Không tải được Database | Sai connection string / SSL | Kiểm tra `DatabaseConnection`, thêm `TrustServerCertificate=True` |
| Handshake không xong | Robot chưa bật / sai COM | Kiểm tra `com`, xem log RX trên MainWindow |
| Timeout chờ Dx | Robot không phản hồi sau lệnh move | Kiểm tra firmware, dây Serial, nguồn robot |
| Tất cả slot FULL | Buffer đầy | Chờ warehouse xử lý (C1x/C2x đã gửi), reset `FullState` khi lấy hàng |
| Gửi C1x/C2x thất bại | Sai `warehouseCom` | Kiểm tra cổng COM22 và thiết bị warehouse |

---

## 13. Thứ tự triển khai Database (lần đầu)

```text
1. Database/01_RobotConfig_Schema.sql
2. Database/02_RobotConfig_SeedData.sql
3. Database/03_RobotConfig_StoredProcedures.sql
```

Chạy trên đúng database trong `DatabaseConnection` (ví dụ `SmartWarehouse`).

---

*Tài liệu đồng bộ với codebase Haui.PCB — cập nhật khi thay đổi luồng robot hoặc giao thức Serial.*
