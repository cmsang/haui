namespace Haui.PCB.Models.Configuration;

/// <summary>
/// GigE transport / pylon stream tuning — section <c>CameraGigEStream</c> in <c>setting.json</c>.
/// Bench camera runs at max ~8 FPS; inter-packet delay mainly reduces sporadic packet loss, not display FPS.
/// </summary>
public sealed class CameraGigEStreamSettings
{
    public const int DefaultInterPacketDelay = 1000;
    public const int DefaultMaxNumBuffer = 24;
    public const int DefaultOutputQueueSize = 2;
    public const int DefaultMaxTransferSizeMb = 4;
    public const int DefaultMaxBufferSizeMb = 64;
    public const int DefaultGrabLoopThreadPriority = 25;
    public const int DefaultConsecutiveFailThreshold = 15;

    /// <summary>GenICam GevSCPD (inter-packet delay) in timestamp ticks. 0 = do not write.</summary>
    public int InterPacketDelay { get; set; } = DefaultInterPacketDelay;

    /// <summary>When true, enables PLStream.AutoPacketSize before optional manual packet size.</summary>
    public bool AutoPacketSize { get; set; } = true;

    /// <summary>GenICam GevSCPSPacketSize in bytes; used only when <see cref="AutoPacketSize"/> is false. 0 = skip.</summary>
    public int PacketSize { get; set; }

    /// <summary>PLCameraInstance.MaxNumBuffer — number of image buffers pre-allocated by pylon.</summary>
    public int MaxNumBuffer { get; set; } = DefaultMaxNumBuffer;

    /// <summary>Output queue depth for GrabStrategy.LatestImages (typically 2–3 for live preview).</summary>
    public int OutputQueueSize { get; set; } = DefaultOutputQueueSize;

    /// <summary>PLStream.MaxTransferSize limit in megabytes.</summary>
    public int MaxTransferSizeMb { get; set; } = DefaultMaxTransferSizeMb;

    /// <summary>PLStream.MaxBufferSize limit in megabytes.</summary>
    public int MaxBufferSizeMb { get; set; } = DefaultMaxBufferSizeMb;

    /// <summary>PLStream.GrabLoopThreadPriority (0–31 on Windows). 0 = do not set.</summary>
    public int GrabLoopThreadPriority { get; set; } = DefaultGrabLoopThreadPriority;

    /// <summary>Consecutive grab failures before auto StreamGrabber restart.</summary>
    public int ConsecutiveFailThreshold { get; set; } = DefaultConsecutiveFailThreshold;

    /// <summary>When true, restart grabber after <see cref="ConsecutiveFailThreshold"/> consecutive failures (throttled).</summary>
    public bool EnableAutoRecovery { get; set; } = true;

    public CameraGigEStreamSettings Clone() => new()
    {
        InterPacketDelay = InterPacketDelay,
        AutoPacketSize = AutoPacketSize,
        PacketSize = PacketSize,
        MaxNumBuffer = MaxNumBuffer,
        OutputQueueSize = OutputQueueSize,
        MaxTransferSizeMb = MaxTransferSizeMb,
        MaxBufferSizeMb = MaxBufferSizeMb,
        GrabLoopThreadPriority = GrabLoopThreadPriority,
        ConsecutiveFailThreshold = ConsecutiveFailThreshold,
        EnableAutoRecovery = EnableAutoRecovery
    };
}
