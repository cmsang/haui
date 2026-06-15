using Basler.Pylon;

namespace Haui.PCB.Processing.Camera;

/// <summary>
/// Shared pylon runtime for GigE: keeps <see cref="Library"/> alive and caches enumerated devices.
/// </summary>
internal static class BaslerPylonRuntime
{
    private static readonly Lock Sync = new();
    private static Library? _library;
    private static readonly HashSet<string> AnnouncedIps = new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, ICameraInfo> _devicesBySerial = new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, ICameraInfo> _devicesByIp = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, (int MaxW, int MaxH)> ResolutionByDevice = new(StringComparer.OrdinalIgnoreCase);

    public static void AnnounceIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return;

        string normalized = ip.Trim();
        lock (Sync)
        {
            EnsureLibrary();
            if (AnnouncedIps.Add(normalized))
                IpConfigurator.AnnounceRemoteDevice(normalized);
        }
    }

    public static IReadOnlyList<ICameraInfo> EnumerateGigE()
    {
        lock (Sync)
        {
            EnsureLibrary();
            var devices = CameraFinder.Enumerate(DeviceType.GigE);
            RebuildCache(devices);
            return devices;
        }
    }

    public static ICameraInfo? FindGigEByIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return null;

        string normalized = ip.Trim();
        lock (Sync)
        {
            if (_devicesByIp.TryGetValue(normalized, out ICameraInfo? cached))
                return cached;

            EnsureLibrary();
            foreach (var device in CameraFinder.Enumerate(DeviceType.GigE))
            {
                string? deviceIp = device[CameraInfoKey.DeviceIpAddress];
                if (!string.Equals(deviceIp, normalized, StringComparison.OrdinalIgnoreCase))
                    continue;

                RegisterDevice(device);
                return device;
            }
        }

        return null;
    }

    public static bool TryGetCachedDevice(string serialOrIp, out ICameraInfo device)
    {
        lock (Sync)
        {
            if (_devicesBySerial.TryGetValue(serialOrIp, out device!))
                return true;
            return _devicesByIp.TryGetValue(serialOrIp, out device!);
        }
    }

    public static void CacheResolution(string deviceKey, int maxW, int maxH)
    {
        if (string.IsNullOrWhiteSpace(deviceKey) || maxW <= 0 || maxH <= 0)
            return;

        lock (Sync)
            ResolutionByDevice[deviceKey.Trim()] = (maxW, maxH);
    }

    public static bool TryGetCachedResolution(string deviceKey, out int maxW, out int maxH)
    {
        lock (Sync)
        {
            if (ResolutionByDevice.TryGetValue(deviceKey, out var value))
            {
                maxW = value.MaxW;
                maxH = value.MaxH;
                return true;
            }
        }

        maxW = 0;
        maxH = 0;
        return false;
    }

    private static void EnsureLibrary()
    {
        _library ??= new Library();
    }

    private static void RebuildCache(IReadOnlyList<ICameraInfo> devices)
    {
        _devicesBySerial = new Dictionary<string, ICameraInfo>(StringComparer.OrdinalIgnoreCase);
        _devicesByIp = new Dictionary<string, ICameraInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var device in devices)
            RegisterDevice(device);
    }

    private static void RegisterDevice(ICameraInfo device)
    {
        string? serial = device[CameraInfoKey.SerialNumber];
        if (!string.IsNullOrEmpty(serial))
            _devicesBySerial[serial] = device;

        string? ip = device[CameraInfoKey.DeviceIpAddress];
        if (!string.IsNullOrEmpty(ip))
            _devicesByIp[ip] = device;
    }
}
