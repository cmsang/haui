using OpenCvSharp;

namespace Haui.GTObjectDetector.Processing;

/// <summary>
/// Giao diện trừu tượng cho dịch vụ camera: dò tìm, kết nối, lấy frame.
/// Tuân theo Dependency Inversion Principle — tầng trên phụ thuộc vào abstraction này.
/// </summary>
public interface ICameraService : IDisposable
{
    /// <summary>Trạng thái camera đang chạy hay không.</summary>
    bool IsRunning { get; }

    /// <summary>Sự kiện phát mỗi khi có frame mới từ camera.</summary>
    event Action<Mat>? FrameArrived;

    /// <summary>Dò tìm tất cả camera có sẵn trên máy bất đồng bộ.</summary>
    Task<IReadOnlyList<CameraInfo>> EnumerateCamerasAsync();

    /// <summary>Lấy danh sách độ phân giải hỗ trợ của camera bất đồng bộ.</summary>
    Task<IReadOnlyList<ResolutionInfo>> GetSupportedResolutionsAsync(string monikerString);

    /// <summary>Bắt đầu kết nối và capture từ camera.</summary>
    void Start(string monikerString, int width, int height);

    /// <summary>Dừng camera.</summary>
    void Stop();

    /// <summary>Trả về clone của frame cuối cùng nhận được.</summary>
    Mat? GrabFrame();

    /// <summary>
    /// Thu thập frame trong <paramref name="durationMs"/> mili-giây, đo độ sắc nét từng frame
    /// bằng phương sai Laplacian, rồi trả về frame sắc nét nhất (clone).
    /// Trả về null nếu camera chưa chạy hoặc không nhận được frame nào.
    /// </summary>
    Task<Mat?> CaptureSharpestFrameAsync(int durationMs = 1500, CancellationToken cancellationToken = default);
}
