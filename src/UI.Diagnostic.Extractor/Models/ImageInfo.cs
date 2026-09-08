namespace UI.Diagnostic.Extractor.Models;

public sealed class ImageInfo
{
    public string FileName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }

    public int Width { get; init; }
    public int Height { get; init; }

    public string Format { get; init; } = string.Empty;
}
