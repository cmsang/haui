using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Haui.PCB.Models.Configuration;
using Haui.PCB.Processing;
using Haui.PCB.Processing.Configuration;
using Haui.PCB.Processing.Segmentation;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Haui.PCB.Views.Windows;

/// <summary>
/// Developer-mode Canny threshold tuner — live preview of Canny + Morphology Close on a captured frame.
/// </summary>
public partial class CannyThresholdWindow : System.Windows.Window
{
    private readonly Mat _sourceFrame;
    private readonly DispatcherTimer _previewTimer = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private readonly IAppSettingService _appSettingService = new AppSettingService();
    private int _previewVersion;
    private bool _syncingUi;

    public CannyThresholdWindow(Mat frame, System.Windows.Window owner)
    {
        _sourceFrame = frame.Clone();
        InitializeComponent();
        Owner = owner;

        var settings = AppSettingsStore.LoadSegmentation();
        SetThresholdUi(settings.CannyThreshold1, settings.CannyThreshold2);

        SourceImage.Source = ToFrozenBitmap(_sourceFrame);

        _previewTimer.Tick += async (_, _) =>
        {
            _previewTimer.Stop();
            await UpdatePreviewAsync();
        };

        Closed += (_, _) =>
        {
            _previewTimer.Stop();
            _sourceFrame.Dispose();
        };

        Loaded += async (_, _) => await UpdatePreviewAsync();
    }

    private void SetThresholdUi(double t1, double t2)
    {
        _syncingUi = true;
        SliderT1.Value = Math.Clamp(t1, SliderT1.Minimum, SliderT1.Maximum);
        SliderT2.Value = Math.Clamp(t2, SliderT2.Minimum, SliderT2.Maximum);
        TextT1.Text = ((int)SliderT1.Value).ToString(CultureInfo.InvariantCulture);
        TextT2.Text = ((int)SliderT2.Value).ToString(CultureInfo.InvariantCulture);
        _syncingUi = false;
    }

    private void Threshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_syncingUi) return;

        _syncingUi = true;
        if (sender == SliderT1)
            TextT1.Text = ((int)SliderT1.Value).ToString(CultureInfo.InvariantCulture);
        else if (sender == SliderT2)
            TextT2.Text = ((int)SliderT2.Value).ToString(CultureInfo.InvariantCulture);
        _syncingUi = false;

        SchedulePreviewUpdate();
    }

    private void TextT1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_syncingUi) return;
        if (!double.TryParse(TextT1.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return;

        _syncingUi = true;
        SliderT1.Value = Math.Clamp(value, SliderT1.Minimum, SliderT1.Maximum);
        _syncingUi = false;
        SchedulePreviewUpdate();
    }

    private void TextT2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_syncingUi) return;
        if (!double.TryParse(TextT2.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return;

        _syncingUi = true;
        SliderT2.Value = Math.Clamp(value, SliderT2.Minimum, SliderT2.Maximum);
        _syncingUi = false;
        SchedulePreviewUpdate();
    }

    private void SchedulePreviewUpdate()
    {
        _previewTimer.Stop();
        _previewTimer.Start();
    }

    private async Task UpdatePreviewAsync()
    {
        var t1 = SliderT1.Value;
        var t2 = SliderT2.Value;
        var version = Interlocked.Increment(ref _previewVersion);

        BitmapSource? cannyBitmap;
        BitmapSource? morphBitmap;
        string detectionNote;

        try
        {
            (cannyBitmap, morphBitmap, detectionNote) = await Task.Run(() =>
            {
                var result = CannyPreviewHelper.Compute(_sourceFrame, t1, t2);
                try
                {
                    var note = result.DetectionSize is { } size
                        ? $" | detection {size.Width}×{size.Height}"
                        : string.Empty;
                    return (ToFrozenBitmap(result.Edges), ToFrozenBitmap(result.Closed), note);
                }
                finally
                {
                    result.DisposeAll();
                }
            });
        }
        catch
        {
            StatusText.Text = "Lỗi tính toán Canny.";
            return;
        }

        if (version != _previewVersion)
            return;

        CannyImage.Source = cannyBitmap;
        MorphImage.Source = morphBitmap;
        StatusText.Text = $"Threshold1={t1:F0}, Threshold2={t2:F0}{detectionNote}";
    }

    private static BitmapSource ToFrozenBitmap(Mat mat)
    {
        var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
        bitmap.Freeze();
        return bitmap;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var settings = new SegmentationPipelineSettings
        {
            CannyThreshold1 = SliderT1.Value,
            CannyThreshold2 = SliderT2.Value
        };
        _appSettingService.SaveSegmentation(settings);
        StatusText.Text = "Đã lưu vào Config/setting.json.";
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
