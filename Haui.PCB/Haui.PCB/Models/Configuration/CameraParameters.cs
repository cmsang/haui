namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Tham số camera Basler — section <c>CameraBasler</c> trong <c>setting.json</c>; UI chính có thể chỉnh tạm thời.
/// </summary>
public class CameraParameters
{
    /// <summary>Static GigE IP — skips broadcast discovery and connects directly when set.</summary>
    public string DeviceIp { get; set; } = string.Empty;

    public double ExposureTimeUs { get; set; } = 15_000;
    public double GainDb { get; set; }
    public double Gamma { get; set; } = 1.0;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1200;
    public string PixelFormat { get; set; } = "Mono8";
    public string GainAuto { get; set; } = "Continuous";
    public string BalanceWhiteAuto { get; set; } = "Continuous";

    public CameraParameters Clone() => new()
    {
        DeviceIp = DeviceIp,
        ExposureTimeUs = ExposureTimeUs,
        GainDb = GainDb,
        Gamma = Gamma,
        Width = Width,
        Height = Height,
        PixelFormat = PixelFormat,
        GainAuto = GainAuto,
        BalanceWhiteAuto = BalanceWhiteAuto
    };
}
