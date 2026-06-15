# Basler Camera (Pylon .NET) — Reference

Quick reference for connecting Basler cameras (GigE / USB3) via **Basler.Pylon** in C# / WPF. Covers **Haui.PCB** implementation and external samples.

---

## SDK & project wiring (Haui.PCB)

| Item | Value |
|------|-------|
| DLL | `Basler.Pylon.dll` from `$(PylonRoot)` — default `C:\Program Files\Basler\pylon` |
| Path | `Development\Assemblies\Basler.Pylon\net8.0\x64\Basler.Pylon.dll` |
| Platform | **x64** only |
| Service | `Haui.PCB/Processing/Camera/BaslerCameraService.cs` |
| Interface | `ICameraService`, `ICameraParameterService` |
| Defaults | `Config/setting.json` → `CameraBasler` via `CameraDefaultsLoader` |
| UI flow | `MainViewModel`: `RefreshCamerasAsync` → `LoadResolutionsAsync` → `StartCamera` |

**NuGet note (Stack Overflow / community):** NuGet packages like `Basler.Pylon.NET.x64` may not ship the real DLL. Prefer a **file reference** to the DLL installed with the full Pylon runtime, and ensure Pylon is installed on the deployment machine.

Sources:
- [Stack Overflow — Connect to Basler Camera in C#](https://stackoverflow.com/questions/77443315/connect-to-basler-camera-in-c-sharp)
- [Basler .NET Programmer's Guide](https://docs.baslerweb.com/dot-net-prog-guide)

---

## Haui.PCB connection flow

```csharp
// 1. Enumerate (serial = DeviceId)
foreach (var info in CameraFinder.Enumerate())
{
    string serial = info[CameraInfoKey.SerialNumber];
    string name   = info[CameraInfoKey.FriendlyName];
}

// 2. Start (BaslerCameraService.Start)
_camera = new PylonCamera(serialNumber);
_camera.Open();
Basler.Pylon.Configuration.AcquireContinuous(_camera, null);
ConfigureStream();          // GigE buffers
TryConfigurePixelFormat();  // Bayer* → BGR8 → Mono8
ConfigureRoi(width, height);
ApplyExposureParameters(_pendingParameters);
_camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
_camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
```

### Stream settings (Haui.PCB)

| Parameter | Value |
|-----------|-------|
| `PLStream.AutoPacketSize` | `true` |
| `PLCameraInstance.MaxNumBuffer` | `16` |
| `PLStream.MaxTransferSize` | 4 MB |
| `PLStream.MaxBufferSize` | 64 MB |

### Pixel format priority (Haui.PCB)

`BayerRG8` → `BayerBG8` → `BayerGR8` → `BayerGB8` → `BGR8` → `RGB8` → `Mono8`

Convert to OpenCV: `PixelDataConverter` with `OutputPixelFormat = PixelType.BGR8packed` → `Mat` CV_8UC3.

### Grab strategy (Haui.PCB)

`GrabStrategy.LatestImages` + `GrabLoop.ProvidedByStreamGrabber` — keeps only the newest frames (good for live preview).

### Default parameters (`setting.json` → `CameraBasler`)

```json
{
  "exposureTimeUs": 20000,
  "gainDb": 0,
  "gamma": 1,
  "width": 2304,
  "height": 1536,
  "balanceWhiteAuto": "Once"
}
```

GenICam mapping: `ExposureTime`, `Gain`, `Gamma`, `Width`/`Height`/`OffsetX`/`OffsetY`, `BalanceWhiteAuto` (`Off` | `Once` | `Continuous`).

---

## Official Basler samples (internet)

Installed with Pylon under `Development\Samples\C#\` and documented online.

| Sample | Purpose |
|--------|---------|
| **Grab** | Basic `RetrieveResult` loop |
| **Grab Using Grab Loop Thread** | `ImageGrabbed` + `ProvidedByStreamGrabber` |
| **Grab Strategies** | `OneByOne`, `LatestImages`, `OutputQueueSize` |
| **Parameterize Camera** | AOI, pixel format, gain auto off |
| **Utility_AnnouceRemoteDevice** | GigE behind router (`AnnounceRemoteDevice`) |
| **Pylon Live View** | WPF GUI + parameter events |

Docs:
- [pylon .NET Programmer's Guide](https://docs.baslerweb.com/pylonapi/net/Guide)
- [.NET Programmer's Guide (overview)](https://docs.baslerweb.com/dot-net-prog-guide)
- [pylon .NET Samples (full code)](https://docs.baslerweb.com/pylonapi/net/Samples)
- [pylon SDK Samples Manual](https://docs.baslerweb.com/pylonapi/pylon-sdk-samples-manual)

### 1. Open / close (official minimal)

```csharp
using Basler.Pylon;

using (Camera camera = new Camera())  // first device found
{
    camera.Open();
    // use camera
    camera.Close();
}
```

Other constructors: by serial, IP, `ICameraInfo`, `DeviceType.GigE`, `CameraSelectionStrategy.FirstFound`.

### 2. Continuous acquisition setup (official)

```csharp
// Option A: event on CameraOpened (before Open)
camera.CameraOpened += Configuration.AcquireContinuous;
camera.Open();

// Option B: call after Open (Haui.PCB style)
camera.Open();
Basler.Pylon.Configuration.AcquireContinuous(camera, null);
```

### 3. Synchronous grab loop (official — Grab sample)

```csharp
camera.StreamGrabber.Start();

for (int i = 0; i < 10; ++i)
{
    IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(
        5000, TimeoutHandling.ThrowException);
    using (grabResult)
    {
        if (grabResult.GrabSucceeded)
        {
            byte[] buffer = grabResult.PixelData as byte[];
            // process buffer
        }
        else
        {
            Console.WriteLine("Error: {0} {1}",
                grabResult.ErrorCode, grabResult.ErrorDescription);
        }
    }
}

camera.StreamGrabber.Stop();
```

**Important:** Always `using (grabResult)` or `Dispose()` — und disposed buffers stop grabbing (buffer underrun).

### 4. Event-driven grab loop thread (official — matches Haui.PCB pattern)

```csharp
static void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
{
    IGrabResult grabResult = e.GrabResult;
    if (grabResult.GrabSucceeded)
    {
        // grabResult disposed when handler returns — clone if keeping data
        byte[] buffer = grabResult.PixelData as byte[];
    }
    else
    {
        Console.WriteLine("Error: {0} {1}",
            grabResult.ErrorCode, grabResult.ErrorDescription);
    }
}

camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
// ... later
camera.StreamGrabber.Stop();
```

**Clone when async processing:**

```csharp
using IGrabResult result = e.GrabResult.Clone();
```

### 5. Grab strategies (official — Grab Strategies sample)

```csharp
// Latest frames only (preview) — same family as Haui.PCB
camera.Parameters[PLCameraInstance.OutputQueueSize].SetValue(2);
camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

// OneByOne: FIFO, no skip
camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
```

| Strategy | Behavior |
|----------|----------|
| `OneByOne` | FIFO; pauses if results not retrieved |
| `LatestImages` | Output queue keeps N newest; older frames skipped |
| `LatestImageOnly` | Equivalent to `LatestImages` + `OutputQueueSize = 1` |

Haui.PCB uses `LatestImages` without explicitly setting `OutputQueueSize`.

### 6. Parameterize camera (official)

```csharp
camera.Open();

// AOI
camera.Parameters[PLCamera.OffsetX].TrySetToMinimum();
camera.Parameters[PLCamera.OffsetY].TrySetToMinimum();
camera.Parameters[PLCamera.Width].SetValue(202, IntegerValueCorrection.Nearest);
camera.Parameters[PLCamera.Height].SetValue(101, IntegerValueCorrection.Nearest);

// Pixel format — use TrySetValue for safety
camera.Parameters[PLCamera.PixelFormat].TrySetValue(PLCamera.PixelFormat.Mono8);

// Disable auto before manual gain
camera.Parameters[PLCamera.GainAuto].TrySetValue(PLCamera.GainAuto.Off);

// Buffer count (default 10)
camera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(16);
```

Use `IsWritable` / `IsReadable` / `TrySetValue` — parameters may be empty on some models.

### 7. PixelDataConverter → Bitmap (official)

```csharp
var converter = new PixelDataConverter();
Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
BitmapData bmpData = bitmap.LockBits(
    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
    ImageLockMode.ReadWrite, bitmap.PixelFormat);
converter.OutputPixelFormat = PixelType.BGRA8packed;
converter.Convert(bmpData.Scan0, bmpData.Stride * bitmap.Height, grabResult);
bitmap.UnlockBits(bmpData);
```

Haui.PCB variant: `PixelType.BGR8packed` → raw `Mat.Data` buffer.

### 8. Enumerate cameras (official)

```csharp
// All devices
List<ICameraInfo> devices = CameraFinder.Enumerate();

// GigE only
List<ICameraInfo> gigeDevices = CameraFinder.Enumerate(DeviceType.GigE);

foreach (var device in devices)
{
    Console.WriteLine(device[CameraInfoKey.ModelName]);
    Console.WriteLine(device[CameraInfoKey.SerialNumber]);
    Console.WriteLine(device[CameraInfoKey.FriendlyName]);
    // GigE extras: DeviceIpAddress, SubnetMask, DefaultGateway, DeviceMacAddress
}

using (Camera camera = new Camera(deviceInfo)) { camera.Open(); }
// or
using (Camera camera = new Camera(serialNumber)) { camera.Open(); }
```

### 9. GigE remote device (official — Utility_AnnouceRemoteDevice)

```csharp
using var lib = new Library();  // keep alive while announced
IpConfigurator.AnnounceRemoteDevice("10.1.1.1");
// camera now visible in CameraFinder.Enumerate()
IpConfigurator.RenounceRemoteDevice("10.1.1.1");
```

---

## Community samples (internet)

### WPF live view — MilleVisionNotes

- [Implementing Live View in WPF (.NET 8)](https://millevision.net/implementing-live-view-in-a-wpf-app-for-basler-cameras-pylon-sdk-c-net-8/)
- [Event-driven capture + async saving](https://millevision.net/stabilizing-basler-camera-acquisition-with-event-driven-capture-and-async-saving-c-pylon-sdk/)

Patterns:
- `Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber)` for live preview
- `e.GrabResult.Clone()` before async / cross-thread use
- Update UI via `Dispatcher.InvokeAsync`
- Remove handler (`-=`) before add (`+=`) to avoid duplicates
- Match `PixelType` to camera format (Mono8 vs BGR8)

### Grab loop thread — Allen's blog (Korean)

- [C# + Pylon Grab Loop Thread](https://allensdatablog.tistory.com/entry/C%EA%B3%BC-Basler-Pylon%EC%9D%84-%EC%82%AC%EC%9A%A9%ED%95%9C-%EB%B9%84%EC%A0%84-%EC%BD%94%EB%94%A9-Grab-Loop-Thread%EB%A5%BC-%EC%9D%B4%EC%9A%A9%ED%95%9C-%EC%9D%B4%EB%AF%B8%EC%A7%80-%EC%88%98%EC%A7%91-%EB%B0%8F-%EC%B2%98%EB%A6%AC)

Same official pattern: `ImageGrabbed` + `ProvidedByStreamGrabber`.

### Python (conceptual parity)

- [Python For The Lab — Basler cameras](https://pythonforthelab.com/blog/getting-started-with-basler-cameras)

`EnumerateDevices()` ≈ `CameraFinder.Enumerate()`; `ImageEventHandler.OnImageGrabbed` ≈ `StreamGrabber.ImageGrabbed`.

---

## Haui.PCB vs official patterns

| Topic | Haui.PCB | Official default |
|-------|----------|------------------|
| Device selection | Serial number | `new Camera()` first found |
| Acquisition mode | `AcquireContinuous` after Open | `CameraOpened += AcquireContinuous` |
| Grab mode | Event + grab loop thread | Same, or sync `RetrieveResult` |
| Strategy | `LatestImages` | `OneByOne` in most samples |
| Image output | OpenCV `Mat` BGR8 | `byte[]`, Bitmap, or ImageWindow |
| GigE tuning | `AutoPacketSize`, buffer sizes | Bandwidth Manager / sample-specific |
| ROI | Center-crop | Offset min + explicit width/height |

---

## Common pitfalls

1. **Missing Pylon runtime** — `CameraFinder.Enumerate()` returns empty; catch in Haui.PCB swallows error.
2. **Wrong DLL / NuGet** — use file reference to installed `Basler.Pylon.dll`, not incomplete NuGet.
3. **Grab result lifetime** — dispose or clone; Haui.PCB clones to `_lastFrame` in lock.
4. **Exceptions in `ImageGrabbed`** — stops grab loop when using `ProvidedByStreamGrabber`.
5. **GigE debugger** — official samples warn: heartbeat timeout ~5 min if breaking after `Open()` under debugger.
6. **SFNC version** — GigE vs USB3 differ (`Gain` vs `GainRaw`, `GammaEnable`); use `GetSfncVersion()` or `TrySetValue`.
7. **WPF thread** — process frames off UI thread; Haui.PCB uses `FrameArrived` → ViewModel preview scaling.
8. **Color wrong** — align `PixelDataConverter.OutputPixelFormat` with sensor output (Bayer demosaic handled by converter).

---

## Key files in repo

```
Haui.PCB/Processing/Camera/BaslerCameraService.cs   # main implementation
Haui.PCB/Processing/Camera/ICameraService.cs
Haui.PCB/Processing/Camera/ICameraParameterService.cs
Haui.PCB/Models/Configuration/CameraParameters.cs
Haui.PCB/Models/Camera/CameraInfo.cs
Haui.PCB/Processing/Configuration/CameraDefaultsLoader.cs
Haui.PCB/Config/setting.json                        # CameraBasler section
Haui.PCB/ViewModels/MainViewModel.cs                # UI orchestration
Haui.PCB/Haui.PCB.csproj                            # Basler.Pylon reference
```

---

*Last updated: 2026-06-15 — compiled from Haui.PCB source + Basler official docs/samples + community articles.*
