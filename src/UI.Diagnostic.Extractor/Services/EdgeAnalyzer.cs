using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Diagnostic.Extractor.Models;

namespace UI.Diagnostic.Extractor.Services;

public sealed class EdgeAnalyzer
{
    private const byte BrightThreshold = 220;
    private const byte DarkThreshold = 80;
    private const int MinimumRunLength = 5;
    private const int MaximumRegions = 100;

    public EdgeAnalysis Analyze(string filePath)
    {
        using var image = Image.Load<Rgba32>(filePath);

        var width = image.Width;
        var height = image.Height;
        var brightness = new byte[width * height];

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var offset = y * width;

                for (var x = 0; x < width; x++)
                {
                    var pixel = row[x];

                    brightness[offset + x] =
                        (byte)Math.Clamp(
                            (int)Math.Round(
                                0.299 * pixel.R +
                                0.587 * pixel.G +
                                0.114 * pixel.B),
                            0,
                            255);
                }
            }
        });

        var regions = new List<EdgeRegion>();

        var horizontalTransitions = ScanHorizontal(
            brightness,
            width,
            height,
            regions);

        var verticalTransitions = ScanVertical(
            brightness,
            width,
            height,
            regions);

        var orderedRegions = regions
            .OrderByDescending(r => r.Length)
            .ThenByDescending(r => r.BrightnessDifference)
            .Take(MaximumRegions)
            .ToList();

        return new EdgeAnalysis
        {
            BrightThreshold = BrightThreshold,
            DarkThreshold = DarkThreshold,
            MinimumRunLength = MinimumRunLength,
            HorizontalTransitions = horizontalTransitions,
            VerticalTransitions = verticalTransitions,
            Regions = orderedRegions
        };
    }

    private static int ScanHorizontal(
        byte[] brightness,
        int width,
        int height,
        List<EdgeRegion> regions)
    {
        var count = 0;

        for (var y = 0; y < height; y++)
        {
            var x = 0;

            while (x < width)
            {
                if (brightness[y * width + x] < BrightThreshold)
                {
                    x++;
                    continue;
                }

                var start = x;
                var maxBrightness = brightness[y * width + x];

                while (x < width &&
                       brightness[y * width + x] >= BrightThreshold)
                {
                    maxBrightness = Math.Max(
                        maxBrightness,
                        brightness[y * width + x]);

                    x++;
                }

                var length = x - start;

                if (length < MinimumRunLength)
                    continue;

                var adjacentDark = false;
                var darkBrightness = 255;

                if (start > 0)
                {
                    var value = brightness[y * width + start - 1];

                    if (value <= DarkThreshold)
                    {
                        adjacentDark = true;
                        darkBrightness = Math.Min(
                            darkBrightness,
                            value);
                    }
                }

                if (x < width)
                {
                    var value = brightness[y * width + x];

                    if (value <= DarkThreshold)
                    {
                        adjacentDark = true;
                        darkBrightness = Math.Min(
                            darkBrightness,
                            value);
                    }
                }

                if (!adjacentDark)
                    continue;

                regions.Add(new EdgeRegion
                {
                    X = start,
                    Y = y,
                    Width = length,
                    Height = 1,
                    Orientation = "Horizontal",
                    Length = length,
                    BrightnessDifference =
                        maxBrightness - darkBrightness
                });

                count++;
            }
        }

        return count;
    }

    private static int ScanVertical(
        byte[] brightness,
        int width,
        int height,
        List<EdgeRegion> regions)
    {
        var count = 0;

        for (var x = 0; x < width; x++)
        {
            var y = 0;

            while (y < height)
            {
                if (brightness[y * width + x] < BrightThreshold)
                {
                    y++;
                    continue;
                }

                var start = y;
                var maxBrightness = brightness[y * width + x];

                while (y < height &&
                       brightness[y * width + x] >= BrightThreshold)
                {
                    maxBrightness = Math.Max(
                        maxBrightness,
                        brightness[y * width + x]);

                    y++;
                }

                var length = y - start;

                if (length < MinimumRunLength)
                    continue;

                var adjacentDark = false;
                var darkBrightness = 255;

                if (start > 0)
                {
                    var value = brightness[(start - 1) * width + x];

                    if (value <= DarkThreshold)
                    {
                        adjacentDark = true;
                        darkBrightness = Math.Min(
                            darkBrightness,
                            value);
                    }
                }

                if (y < height)
                {
                    var value = brightness[y * width + x];

                    if (value <= DarkThreshold)
                    {
                        adjacentDark = true;
                        darkBrightness = Math.Min(
                            darkBrightness,
                            value);
                    }
                }

                if (!adjacentDark)
                    continue;

                regions.Add(new EdgeRegion
                {
                    X = x,
                    Y = start,
                    Width = 1,
                    Height = length,
                    Orientation = "Vertical",
                    Length = length,
                    BrightnessDifference =
                        maxBrightness - darkBrightness
                });

                count++;
            }
        }

        return count;
    }
}
