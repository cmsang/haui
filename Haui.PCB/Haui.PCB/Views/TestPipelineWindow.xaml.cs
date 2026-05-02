using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Haui.PCB.Processing;

namespace Haui.PCB.Views;

public partial class TestPipelineWindow : System.Windows.Window
{
    private readonly PcbSegmentationService _segmentation = new();
    private Mat? _sourceMat;

    public TestPipelineWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Nạp ảnh từ bên ngoài (ví dụ từ camera chụp).
    /// </summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();

        OriginalImage.Source = _sourceMat.ToWriteableBitmap();
        OriginalPlaceholder.Visibility = Visibility.Collapsed;

        ProcessedImage.Source = null;
        ProcessedPlaceholder.Visibility = Visibility.Visible;
        StatusText.Text = "Ảnh đã tải. Bấm ▶ Test để xử lý.";
        BtnTest.IsEnabled = true;
    }

    private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn ảnh bo mạch",
            Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        _sourceMat?.Dispose();
        _sourceMat = Cv2.ImRead(dialog.FileName, ImreadModes.Color);

        if (_sourceMat.Empty())
        {
            StatusText.Text = "Không thể đọc ảnh.";
            return;
        }

        OriginalImage.Source = _sourceMat.ToWriteableBitmap();
        OriginalPlaceholder.Visibility = Visibility.Collapsed;

        ProcessedImage.Source = null;
        ProcessedPlaceholder.Visibility = Visibility.Visible;
        ProcessedPlaceholder.Text = "Chưa xử lý";
        StatusText.Text = $"Đã chọn: {Path.GetFileName(dialog.FileName)}";
        BtnTest.IsEnabled = true;
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        if (_sourceMat is null || _sourceMat.Empty())
        {
            StatusText.Text = "Vui lòng chọn ảnh trước.";
            return;
        }

        BtnTest.IsEnabled = false;
        BtnSelectImage.IsEnabled = false;
        StatusText.Text = "Đang xử lý...";

        Mat? result = null;
        var source = _sourceMat.Clone();

        try
        {
            // Chạy phân vùng trên thread nền để không block UI
            result = await Task.Run(() => _segmentation.Segment(source));

            if (result is null)
            {
                StatusText.Text = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedPlaceholder.Text = "Không tìm thấy bo mạch";
                ProcessedPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                ProcessedImage.Source = result.ToWriteableBitmap();
                ProcessedPlaceholder.Visibility = Visibility.Collapsed;
                StatusText.Text = $"Hoàn thành. Kích thước PCB: {result.Width}×{result.Height} px";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Lỗi: {ex.Message}";
        }
        finally
        {
            source.Dispose();
            result?.Dispose();
            BtnTest.IsEnabled = true;
            BtnSelectImage.IsEnabled = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _sourceMat?.Dispose();
    }
}
