namespace Haui.PCB.Models.Configuration;

/// <summary>
/// Cấu hình thư viện mẫu linh kiện — section <c>ComponentTemplates</c> trong <c>setting.json</c>.
/// </summary>
public sealed class ComponentTemplateSettings
{
    public const string DefaultLibraryFolder = "templates";

    public const string DefaultWhiteCircuitLibraryFolder = "white_circuit_templates";

    /// <summary>Ngưỡng % tương đồng tối thiểu để coi vùng là giống mẫu (Test pipeline).</summary>
    public const double DefaultMinMatchSimilarityPercent = 80.0;

    /// <summary>Tên vùng hợp lệ khi tạo mẫu (mặc định theo sơ đồ linh kiện).</summary>
    public static readonly string[] DefaultAllowedRegionNames =
    [
        "KF1", "KF2", "KF3", "C2", "AMS1", "L3", "L1", "R1", "R2",
        "C3", "C6", "C5", "D2", "D1", "C1", "LM1", "C4"
    ];

    /// <summary>Thư mục thư viện mẫu (đường dẫn tuyệt đối hoặc tương đối CWD). Rỗng → <see cref="DefaultLibraryFolder"/>.</summary>
    public string CustomFolder { get; set; } = string.Empty;

    /// <summary>Danh sách tên vùng được phép khi tạo / chỉnh sửa mẫu.</summary>
    public List<string> AllowedRegionNames { get; set; } =
        [.. DefaultAllowedRegionNames];

    /// <summary>Ngưỡng % tương đồng tối thiểu (0..100) khi so sánh vùng linh kiện.</summary>
    public double MinMatchSimilarityPercent { get; set; } = DefaultMinMatchSimilarityPercent;

    /// <summary>
    /// Allowed region name of the component used to determine board orientation during inspection.
    /// Empty → skip automatic orientation detection.
    /// </summary>
    public string OrientationComponentName { get; set; } = string.Empty;

    public const double DefaultMinOrientationMatchScore = 0.55;

    /// <summary>Minimum CCoeffNormed score (0..1) for orientation component match.</summary>
    public double MinOrientationMatchScore { get; set; } = DefaultMinOrientationMatchScore;

    /// <summary>When true, inspection and template tools use white-circuit library and inverted PASS logic.</summary>
    public bool TrainWhiteCircuit { get; set; }

    /// <summary>White-circuit template library folder (absolute or CWD-relative). Empty → <see cref="DefaultWhiteCircuitLibraryFolder"/>.</summary>
    public string WhiteCircuitCustomFolder { get; set; } = string.Empty;
}
