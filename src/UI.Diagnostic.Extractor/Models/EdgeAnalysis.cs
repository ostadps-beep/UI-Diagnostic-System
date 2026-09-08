namespace UI.Diagnostic.Extractor.Models;

public sealed class EdgeAnalysis
{
    public int BrightThreshold { get; init; }
    public int DarkThreshold { get; init; }
    public int MinimumRunLength { get; init; }

    public long HorizontalTransitions { get; init; }
    public long VerticalTransitions { get; init; }

    public List<EdgeRegion> Regions { get; init; } = [];
}

public sealed class EdgeRegion
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public string Orientation { get; init; } = string.Empty;

    public int Length { get; init; }
    public int BrightnessDifference { get; init; }
}
