# Tài liệu luồng hoạt động Robot — Haui.PCB

Tài liệu mô tả các luồng điều khiển robot 5 khớp RRRRR + gripper trong hệ thống Pick & Place PCB, giao tiếp qua SerialPort và lưu cấu hình vị trí trên SQL Server.

---

## 1. Tổng quan hệ thống

| Thành phần | Mô tả |
|------------|--------|
| **Robot** | Cánh tay 5 DOF (J1–J5) + gripper, firmware nhận lệnh ASCII qua COM |
| **Warehouse** | Thiết bị/PLC phụ trên cổng `warehouseCom` — CMx/COx trước khi robot gắp; C1/C2 khi buffer đầy |
| **SQL Server** | Bảng `RobotConfig` — lưu tọa độ teach và trạng thái slot (`EMPTY` / `FULL`) |
| **Ứng dụng** | WPF `Haui.PCB` — MainWindow, Robot Teaching, Manual Control |

### Kiến trúc phần mềm

```
┌─────────────────────────────────────────────────────────────┐
│  UI (WPF)                                                    │
│  MainWindow · wdTeaching · wdManualControl                   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  Processing (Haui.PCB)                                       │
│  RobotSerialService · RobotPickPlaceExecutor                 │
│  MaterialTransferService · WarehouseSerialService            │
│  RobotStartupHandshakeService · RobotPositionTracker         │
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
| `G40x`, `G20x` | Mở / đóng gripper (góc độ — mở 40°, đóng 20°) |
| `Dx` | Robot báo **hoàn thành** bước di chuyển (nhận) |

### Phản hồi homing

| Nhận | Ý nghĩa |
|------|---------|
| `Ax` | Bắt đầu homing trục |
| `Dx` | Hoàn thành homing / hoàn thành lệnh move |

### Lệnh warehouse (cổng `warehouseCom`)

| Lệnh | Ý nghĩa |
|------|---------|
| `CMx` | PC yêu cầu chuyển material (trước khi robot gắp) |
| `COx` | Nhà kho xác nhận sẵn sàng — PC mới chạy robot |
| `C1x` | Tất cả slot **OK1–OK4** có `FullState = FULL` |
| `C2x` | Tất cả slot **NG1–NG4** có `FullState = FULL` |

---

## 4. Danh sách vị trí teach chuẩn

| Nhóm | Tên vị trí | Vai trò |
|------|------------|---------|
| Chung | `PickUp` | Điểm gắp PCB |
| Chung | `Wait PickUp` | Chờ trước/sau gắp — lưu DB, teach thủ công |
| Chung | `Wait` | Hành lang giữa pick và place — lưu DB, teach được |
| Chung | `Wait OK1` … `Wait OK4` | Chờ trước/sau đặt **Pass** từng slot — teach thủ công |
| Chung | `Wait NG1` … `Wait NG4` | Chờ trước/sau đặt **Fail** từng slot — teach thủ công |
| OK | `OK1` … `OK4` | Buffer hàng **Pass** |
| NG | `NG1` … `NG4` | Buffer hàng **Fail** |

Mọi vị trí **Wait** được teach thủ công như các vị trí khác — chọn trong bảng, chỉnh khớp rồi **Teach**. Khi place vào **OK2**, chu trình dùng **Wait OK2** (bước 6, 9); tương tự **NG3** → **Wait NG3**.

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
7. Handshake xong (`IsCompleted`) → đặt `RobotPositionTracker = Home` (mục 8.6).

---

## 7. Luồng Teach vị trí (Robot Teaching)

**Màn hình:** `wdTeaching`  
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
6. Mọi jog/move/goto/homing thủ công đặt `RobotPositionTracker = Unknown`; khi đóng màn hình robot homing về Home → `Home` (mục 8.6).

---

## 8. Luồng Manual Control

**Màn hình:** `wdManualControl`  
**ViewModel:** `ManualControlViewModel`  
**Executor:** `RobotPickPlaceExecutor`

Dùng để **test thủ công** chu trình gắp–đặt tới một slot OK hoặc NG do người dùng chọn.

### Chu trình 11 bước

| Bước | Hành động |
|------|-----------|
| 1/11 | Move → **Wait PickUp** (chỉ bắt đầu khi robot ở **Home** hoặc **Wait** — xem mục 8.6) |
| 2/11 | `G40x` — Mở gripper |
| 3/11 | Move → **PickUp** |
| 4/11 | `G20x` — Đóng gripper (gắp) |
| 5/11 | Move → **Wait PickUp** (rút lui) |
| 6/11 | Move → **Wait OKx** hoặc **Wait NGx** (theo slot đích, vd OK2 → Wait OK2) |
| 7/11 | Move → **OKx / NGx** (Place) |
| 8/11 | `G40x` — Mở gripper (thả) |
| 9/11 | Move → **Wait OKx/NGx** (rút lui — cùng điểm bước 6) |
| 10/11 | Move → **Wait** |
| 11/11 | `G20x` — Đóng gripper |

**Wait PickUp** / **Wait OK1–4** / **Wait NG1–4** / **Wait** — đều từ Database (teach thủ công). Mỗi slot OK/NG có điểm wait riêng để tránh va đập.

- **Điều kiện xuất phát**: chu trình **chỉ chạy khi robot ở Home hoặc Wait** (xem mục 8.6). Vị trí bất kỳ → bị chặn, báo lỗi, không gửi lệnh.
- Bước **Move / Home**: đăng ký lắng nghe **trước** khi gửi, chỉ chấp nhận `Dx` **sau** khi TX; timeout **120 giây**/bước, **thử lại 1 lần** nếu timeout.
- Bước **gripper** (`G…x`): chờ `Dx` tối đa **5 giây**; luôn chờ thêm **500 ms** ổn định servo trước bước tiếp (tránh `Dx` trễ từ gripper làm kẹt bước Move).
- Manual Control **không** cập nhật `FullState` trong Database.

---

## 8.5. Luồng Auto (CAPx → nhận dạng → robot)

**Luồng Auto** = nhà kho gửi `CAPx` → PC chụp + nhận dạng → robot phân loại. Không cần nhấn Pass/Fail thủ công.

```mermaid
flowchart TD
    WH[Warehouse gửi CAPx] --> CAP[WarehouseSerialService.CaptureRequested]
    CAP --> INS[DashboardTabView.RequestInspection]
    INS --> CAM[Chụp frame + InspectAsync]
    CAM --> RES{PASS / FAIL}
    RES --> HND[MainWindow.HandleInspectionCompletedAsync]
    HND --> TR[MaterialTransferService.TransferAsync]
    TR --> CM[CMx → chờ COx]
    CM --> RB[Chu trình 11 bước Pick & Place]
    RB --> FULL[Cập nhật FullState = FULL]
```

| Bước | Thành phần | Ghi chú |
|------|------------|---------|
| 1 | `CAPx` trên `warehouseCom` | `WarehouseSerialService` lắng nghe liên tục |
| 2 | `TryRequestInspection()` | Camera phải chạy; nếu đang Teaching/Manual → **lưu CAPx chờ** (`WarehousePendingCaptureService`), chụp sau khi đóng màn hình |
| 3 | `InspectionCompleted` | PASS/FAIL từ YOLO |
| 4 | `HandleInspectionCompletedAsync` | Robot phải handshake xong (`Rx→Yx→H0x`) |
| 5 | `TransferAsync` | Giống nút Pass/Fail — CMx/COx rồi 11 bước |

**Vị trí xuất phát:** robot **phải đang ở Home hoặc Wait** thì chu trình mới chạy (chốt chặn ở mục 8.6). Nếu đang ở vị trí bất kỳ (vừa jog/move/goto, hoặc chu trình trước lỗi giữa chừng) → bị chặn, báo lỗi, **không** gửi `CMx`. Bước 1 Move tới Wait PickUp, bước 2 mới mở gripper.

**Lỗi thường gặp khi ở home:**

| Triệu chứng | Nguyên nhân | Xử lý |
|-------------|-------------|-------|
| Chỉ thấy `G40x`, không có `M…x` | Phiên bản cũ: mở gripper trước khi Move; `Dx` trễ từ gripper kẹt bước Move | Đã sửa: Move Wait PickUp trước, `Dx` chỉ hợp lệ sau TX |
| Không chạy robot sau nhận dạng | Robot chưa handshake / điểm teach toàn 0 | Teach PickUp, Wait PickUp, Wait, Wait OKx/NGx theo slot, slot đích |
| Báo "robot đang ở vị trí … chỉ chạy khi Home/Wait" | Robot ở vị trí `Unknown` (vừa jog/move/goto) | Homing về Home rồi chạy lại (mục 8.6) |
| `CAPx` nhưng không kiểm tra | Camera chưa Start | Bật camera trên Dashboard trước khi vận hành Auto |
| CAPx bị bỏ qua khi đang Teaching/Manual | Nhà kho chỉ gửi CAP **một lần** | CAPx được **lưu chờ**; tự chụp khi đóng Teaching/Manual (hoặc khi bật camera / hết bận) |

---

## 8.6. Chốt vị trí xuất phát (RobotPositionTracker)

Chu trình 11 bước **chỉ được phép bắt đầu khi cánh tay ở Home hoặc Wait** — không cho lao thẳng tới Wait PickUp từ vị trí bất kỳ.

Firmware **chưa** có lệnh đọc toạ độ thật, nên hệ thống theo dõi **vị trí logic** bằng `RobotPositionTracker` (`Processing/RobotPositionTracker.cs`). Một instance dùng chung do `MainWindow` sở hữu, truyền vào `MaterialTransferService` và hai cửa sổ `wdTeaching` / `wdManualControl`.

| Trạng thái | Ý nghĩa | Cho chạy chu trình? |
|------------|---------|:---:|
| `Home` | Đã homing (H0x) | ✅ |
| `Wait` | Đứng ở Wait sau khi hoàn tất một chu trình | ✅ |
| `Unknown` | Vừa jog/move/goto bất kỳ, hoặc chu trình lỗi giữa chừng | ❌ |

**Cập nhật trạng thái:**

| Sự kiện | Trạng thái mới |
|---------|----------------|
| Handshake khởi động xong (`Rx→Yx→H0x`) | `Home` |
| `ReturnToHomeAsync` xong (đóng màn Teaching/Manual) | `Home` |
| Chu trình Auto/test bắt đầu | `Unknown` |
| Chu trình kết thúc ở bước 11 (Wait) | `Wait` |
| Jog / Move / Go To / Homing thủ công | `Unknown` |

**Chốt chặn** đặt ở đầu `MaterialTransferService.TransferAsync`: nếu `!CanStartCycle` → báo lỗi kèm vị trí hiện tại và **return ngay, không gửi `CMx`, không chạy robot**. Đưa robot về Home (homing) hoặc hoàn tất một chu trình hợp lệ rồi thử lại.

> Khi có giao thức đọc vị trí thật từ firmware, chỉ cần đổi nguồn cập nhật `RobotPositionTracker`; phần chốt chặn giữ nguyên.

---

## 9. Luồng Pass / Fail material (MainWindow)

**Service:** `MaterialTransferService` → `MainViewModel`  
**Warehouse:** `WarehouseSerialService` (CMx/COx, C1x/C2x)

Cả **luồng Auto (CAPx)** và **nút Pass/Fail** đều hội tụ vào **một** hàm `MaterialTransferService.TransferAsync` — đảm bảo luôn gửi CMx và chờ COx trước khi chạy cánh tay robot.

### 9.1. Ba điểm kích hoạt

```mermaid
flowchart LR
    subgraph UI["Kích hoạt (1 trong 3)"]
        I1[Nhận dạng PASS/FAIL<br/>DashboardTabView]
        I2[Nút Pass<br/>MainWindow sidebar]
        I3[Nút Fail<br/>MainWindow sidebar]
    end

    subgraph VM["MainViewModel"]
        V1[TransferMaterialByInspectionResultAsync]
        V2[TransferPassMaterial]
        V3[TransferFailMaterial]
    end

    SVC[MaterialTransferService.TransferAsync]

    I1 --> V1 --> SVC
    I2 --> V2 --> SVC
    I3 --> V3 --> SVC
```

| Nguồn | File / sự kiện | Gọi tới |
|-------|----------------|---------|
| Nhận dạng PASS (Auto CAPx) | `CAPx` → `InspectionCompleted` → `DashboardTabView` → `MainWindow.HandleInspectionCompletedAsync` | `TransferMaterialByInspectionResultAsync(true)` |
| Nhận dạng FAIL (Auto CAPx) | Cùng chuỗi trên | `TransferMaterialByInspectionResultAsync(false)` |
| Nút **Pass** | `MainWindow.btnPass_Click` | `TransferPassMaterial()` |
| Nút **Fail** | `MainWindow.btnFail_Click` | `TransferFailMaterial()` |

**Manual Control** (`wdManualControl`) **không** đi qua luồng này — chỉ test robot thủ công, không gửi CMx/COx.

### 9.2. Chọn slot đích

| Nút / kết quả | Nhóm slot | Quy tắc chọn |
|---------------|-----------|--------------|
| **Pass** / nhận dạng PASS | OK1–OK4 | Slot **đầu tiên** có `FullState = EMPTY` |
| **Fail** / nhận dạng FAIL | NG1–NG4 | Slot **đầu tiên** có `FullState = EMPTY` |

Nếu tất cả slot nhóm đó đã `FULL` → **không** gửi CMx, không chạy robot; chỉ gửi C1x/C2x nếu buffer đầy (mục 9.4) và báo lỗi.

### 9.3. Sơ đồ luồng xử lý (chi tiết)

```
┌─────────────────────────────────────────────────────────────┐
│  KÍCH HOẠT: nhận dạng PASS/FAIL · nút Pass · nút Fail      │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
              MaterialTransferService.TransferAsync
                           │
         ┌─────────────────┼─────────────────┐
         ▼                 ▼                 ▼
    Kiểm tra DB      Tìm slot EMPTY    Kiểm tra teach points
    (PickUp, Wait,   (OK1–4 / NG1–4)  (Wait PickUp, Wait OKx/NGx)
     slot đích…)          │                 │
         └─────────────────┴─────────────────┘
                           │
              ┌────────────┴────────────┐
              │  Còn slot EMPTY?        │
              └────────────┬────────────┘
                    Không  │  Có
              ┌────────────┴────────────┐
              ▼                         ▼
    Gửi C1x/C2x (buffer đầy)    Gửi CMx → warehouseCom
    Dừng — không chạy robot              │
                                         ▼
                              Chờ COx (timeout 120 s)
                                         │
                              ┌──────────┴──────────┐
                         Timeout/lỗi            Nhận COx
                              │                    │
                              ▼                    ▼
                    Dừng — không robot    Kết nối cổng robot (com)
                                                   │
                                                   ▼
                              Chu trình 11 bước (RobotPickPlaceExecutor)
                                                   │
                                                   ▼
                              Cập nhật FullState = FULL (slot vừa đặt)
                                                   │
                                                   ▼
                              Tất cả OK/NG FULL? → gửi C1x/C2x
```

```mermaid
flowchart TD
    A[Nhận dạng PASS/FAIL<br/>hoặc nút Pass/Fail] --> B[Load RobotConfig từ DB]
    B --> V{Đủ teach points<br/>và slot EMPTY?}
    V -->|Không — buffer đầy| D[Gửi C1x hoặc C2x nếu cần]
    D --> Z[Dừng]
    V -->|Không — lỗi khác| Z
    V -->|Có| W[Gửi CMx → warehouseCom]
    W -->|Timeout / lỗi| X[Dừng — không chạy robot]
    W -->|Nhận COx| R[Kết nối cổng robot com]
    R --> E[Chu trình 11 bước Pick & Place]
    E --> F[Cập nhật FullState = FULL]
    F --> G{Tất cả OK/NG đều FULL?}
    G -->|Có| H[Gửi C1x hoặc C2x]
    G -->|Không| I[Hoàn tất]
    H --> I
```

**Thứ tự thực thi:**
0. **Kiểm tra vị trí xuất phát** — chỉ tiếp tục khi robot ở Home hoặc Wait (`RobotPositionTracker.CanStartCycle`, mục 8.6). Vị trí bất kỳ → báo lỗi, dừng, không gửi `CMx`.
1. Load vị trí từ Database — tính Wait PickUp và Wait OKx/NGx theo slot đích.
2. **Gửi `CMx` → `warehouseCom`, chờ `COx`** (timeout 120 s). Chỉ khi nhận `COx` mới tiếp tục.
3. Kết nối robot COM (`com`) nếu chưa online.
4. Chạy **chu trình 11 bước** (`RobotPickPlaceExecutor`).
5. Gọi `Update_RobotConfig_FullState` → slot vừa đặt = **`FULL`**.
6. Nếu toàn bộ OK (hoặc NG) đều `FULL` → gửi `C1x` / `C2x`.

### 9.4. Handshake CMx / COx (sequence)

```mermaid
sequenceDiagram
    participant App as Haui.PCB
    participant WH as Warehouse (warehouseCom)
    participant RB as Robot (com)

    Note over App: Sau nhận dạng hoặc Pass/Fail — đã có slot EMPTY

    App->>WH: CMx
    alt Nhận COx trong 120 s
        WH-->>App: COx
        Note over App: Nhà kho sẵn sàng
        App->>RB: Kết nối COM (nếu chưa)
        loop 11 bước Pick & Place
            App->>RB: M…x (chờ Dx) / G…x (chờ Dx tối đa 5 s)
            RB-->>App: Dx
        end
        Note over App: Cập nhật FullState = FULL
        opt Tất cả slot OK/NG đều FULL
            App->>WH: C1x hoặc C2x
        end
    else Timeout / không COx
        Note over App: Dừng — không gọi robot
    end
```

| Bước | Lệnh | Hướng | Ghi chú |
|------|------|-------|---------|
| 1 | `CMx` | PC → warehouse | Yêu cầu chuyển material |
| 2 | `COx` | warehouse → PC | **Bắt buộc** — mới được chạy robot |
| 3 | M/G + chờ `Dx` | PC ↔ robot | Chu trình 11 bước |
| 4 | `C1x` / `C2x` | PC → warehouse | Chỉ khi buffer đầy sau khi đặt hàng |

Gửi warehouse qua cổng riêng (`warehouseCom`), mở COM → gửi/nhận → đóng — **không** dùng chung cổng robot.

**DeveloperMode + VirtualSerialPort:** bỏ qua CMx/COx (coi như nhà kho OK) — chỉ dùng khi test không có thiết bị warehouse thật.

### 9.5. Báo warehouse buffer đầy

| Điều kiện | Lệnh gửi | Cổng |
|-----------|----------|------|
| OK1–OK4 đều `FULL` | `C1x` | `warehouseCom` |
| NG1–NG4 đều `FULL` | `C2x` | `warehouseCom` |

Xảy ra khi: (a) không còn slot trống trước khi chạy robot, hoặc (b) sau khi đặt hàng làm đầy toàn bộ buffer nhóm đó.

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
        T1[wdTeaching]
        T2[Teach → Update DB J1-J5]
        T1 --> T2
    end

    subgraph Production["Sản xuất — Pass/Fail / nhận dạng"]
        P1[Nhận dạng PCB hoặc nút Pass/Fail]
        P2{Pass?}
        P3[Chọn slot OK EMPTY]
        P4[Chọn slot NG EMPTY]
        P5[CMx → chờ COx]
        P6[Kết nối robot + Pick & Place 11 bước]
        P7[FullState = FULL]
        P8[Buffer đầy? → C1x/C2x]
        P1 --> P2
        P2 -->|Đạt| P3 --> P5
        P2 -->|Lỗi| P4 --> P5
        P5 -->|COx| P6 --> P7 --> P8
        P5 -->|Timeout| P9[Dừng — không robot]
    end

    subgraph Manual["Manual Control (test)"]
        M1[wdManualControl]
        M2[Pick & Place 11 bước — không CMx/COx]
        M1 --> M2
    end

    Startup --> Teaching
    Teaching --> Production
    Teaching --> Manual
```

---

## 11. File mã nguồn liên quan

| File / thư mục | Vai trò |
|----------------|---------|
| `Haui.PCB/Processing/RobotSerialService.cs` | Gửi/nhận COM robot |
| `Haui.PCB/Processing/RobotSerialProtocol.cs` | Định dạng lệnh M/J/H/G/R/C |
| `Haui.PCB/Processing/RobotStartupHandshakeService.cs` | Handshake Rx/Yx/H0 |
| `Haui.PCB/Processing/RobotPickPlaceExecutor.cs` | Chu trình 11 bước + chờ Dx |
| `Haui.PCB/Processing/RobotPositionTracker.cs` | Theo dõi vị trí logic (Home/Wait/Unknown) — chốt chặn chu trình |
| `Haui.PCB/Processing/RobotManualInterventionGate.cs` | Chặn auto khi Teaching/Manual Control đang mở |
| `Haui.PCB/Processing/WarehousePendingCaptureService.cs` | Lưu CAPx chờ khi chưa thể chụp (Teaching/Manual, camera tắt, đang bận) |
| `Haui.PCB/Processing/MaterialTransferService.cs` | Pass/Fail + FULL + warehouse + chốt vị trí |
| `Haui.PCB/Processing/WarehouseSerialService.cs` | CMx/COx handshake, C1x/C2x buffer đầy |
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
| Timeout chờ COx | Nhà kho không phản hồi sau CMx | Kiểm tra `warehouseCom`, firmware warehouse, dây Serial |
| Gửi C1x/C2x thất bại | Sai `warehouseCom` | Kiểm tra cổng COM22 và thiết bị warehouse |
| "Robot đang ở vị trí … chỉ chạy khi Home/Wait" | Robot ở `Unknown` (vừa jog/move/goto hoặc chu trình trước lỗi) | Homing về Home rồi chạy lại (mục 8.6) |

---

## 13. Thứ tự triển khai Database (lần đầu)

```text
1. Database/01_RobotConfig_Schema.sql
2. Database/02_RobotConfig_SeedData.sql
3. Database/03_RobotConfig_StoredProcedures.sql
4. Database/04_RobotConfig_Clone_WaitSlots.sql   -- DB đã có sẵn OK/NG: clone Wait OK1–4 / Wait NG1–4
```

Chạy trên đúng database trong `DatabaseConnection` (ví dụ `SmartWarehouse`).

---

*Tài liệu đồng bộ với codebase Haui.PCB — cập nhật khi thay đổi luồng robot hoặc giao thức Serial.*
