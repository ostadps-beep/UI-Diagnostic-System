namespace UI.Diagnostic.Extractor.Models;

public sealed class ImageStatistics
{
    public long TotalPixels { get; init; }

    public double AverageRed { get; init; }
    public double AverageGreen { get; init; }
    public double AverageBlue { get; init; }

    public double AverageBrightness { get; init; }
    public double MinimumBrightness { get; init; }
    public double MaximumBrightness { get; init; }

    public double DarkPixelPercentage { get; init; }
    public double BrightPixelPercentage { get; init; }

    public int[] BrightnessHistogram { get; init; } = [];

    public List<DominantColor> DominantColors { get; init; } = [];
}

public sealed class DominantColor
{
    public int Red { get; init; }
    public int Green { get; init; }
    public int Blue { get; init; }
    public long PixelCount { get; init; }
    public double Percentage { get; init; }
}
