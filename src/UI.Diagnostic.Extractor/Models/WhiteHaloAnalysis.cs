namespace UI.Diagnostic.Extractor.Models;

public sealed class WhiteHaloAnalysis
{
    public int BrightThreshold { get; init; }
    public int DarkThreshold { get; init; }

    public long CandidatePixels { get; init; }
    public double CandidatePercentage { get; init; }

    public List<WhiteHaloRegion> Regions { get; init; } = [];
}

public sealed class WhiteHaloRegion
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public int PixelCount { get; init; }
    public double Density { get; init; }
}
