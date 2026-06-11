using System.IO;
using Haui.PCB.Models;
using OpenCvSharp;

namespace Haui.PCB.Processing;

/// <summary>
/// Lưu frame camera vào thư mục cấu hình (<c>setting.json</c> → CameraCapture).
/// </summary>
public class CameraCaptureService
{
    public string GetSaveFolder()
    {
        var settings = AppSettingsStore.LoadCameraCapture();
        return string.IsNullOrWhiteSpace(settings.SaveFolder)
            ? CameraCaptureSettings.DefaultSaveFolder
            : settings.SaveFolder.Trim();
    }

    /// <summary>Lưu ảnh PNG; trả về đường dẫn đầy đủ.</summary>
    public string SaveFrame(Mat frame)
    {
        if (frame.Empty())
            throw new ArgumentException("Frame rỗng.", nameof(frame));

        var folder = GetSaveFolder();
        Directory.CreateDirectory(folder);

        var fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
        var filePath = Path.Combine(folder, fileName);
        Cv2.ImWrite(filePath, frame);
        return filePath;
    }
}
