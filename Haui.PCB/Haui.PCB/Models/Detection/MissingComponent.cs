using OpenCvSharp;

namespace Haui.PCB.Models.Detection;

/// <summary>One YOLO detection — a missing component at <see cref="Box"/>.</summary>
public sealed record MissingComponent(string Label, double Confidence, Rect Box);
