namespace Haui.PCB.Models;

/// <summary>
/// Tham số camera Basler hiển thị và chỉnh trên UI chính.
/// </summary>
public class CameraParameters
{
    public double ExposureTimeUs { get; set; } = 15_000;
    public double GainDb { get; set; }
    public double Gamma { get; set; } = 1.0;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1200;
    public string BalanceWhiteAuto { get; set; } = "Off";

    public CameraParameters Clone() => new()
    {
        ExposureTimeUs = ExposureTimeUs,
        GainDb = GainDb,
        Gamma = Gamma,
        Width = Width,
        Height = Height,
        BalanceWhiteAuto = BalanceWhiteAuto
    };
}
