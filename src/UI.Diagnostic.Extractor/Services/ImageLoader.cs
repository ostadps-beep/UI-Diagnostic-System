using SixLabors.ImageSharp;
using UI.Diagnostic.Extractor.Models;

namespace UI.Diagnostic.Extractor.Services;

public sealed class ImageLoader
{
    public UI.Diagnostic.Extractor.Models.ImageInfo Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Image file not found.", filePath);

        var fileInfo = new FileInfo(filePath);

        using var image = Image.Load(filePath);

        return new UI.Diagnostic.Extractor.Models.ImageInfo
        {
            FileName = fileInfo.Name,
            FilePath = fileInfo.FullName,
            FileSizeBytes = fileInfo.Length,
            Width = image.Width,
            Height = image.Height,
            Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown"
        };
    }
}
