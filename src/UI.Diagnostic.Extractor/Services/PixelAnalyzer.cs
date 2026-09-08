using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Diagnostic.Extractor.Models;

namespace UI.Diagnostic.Extractor.Services;

public sealed class PixelAnalyzer
{
    public ImageStatistics Analyze(string filePath)
    {
        using var image = Image.Load<Rgba32>(filePath);

        long totalPixels = (long)image.Width * image.Height;

        long redSum = 0;
        long greenSum = 0;
        long blueSum = 0;

        double brightnessSum = 0;
        double minimumBrightness = 255;
        double maximumBrightness = 0;

        long darkPixels = 0;
        long brightPixels = 0;

        var histogram = new int[256];

        // 16 levels per RGB channel = 4096 color buckets.
        var colorBuckets = new Dictionary<int, long>();

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];

                    int r = pixel.R;
                    int g = pixel.G;
                    int b = pixel.B;

                    redSum += r;
                    greenSum += g;
                    blueSum += b;

                    double brightness =
                        (0.2126 * r) +
                        (0.7152 * g) +
                        (0.0722 * b);

                    brightnessSum += brightness;

                    if (brightness < minimumBrightness)
                        minimumBrightness = brightness;

                    if (brightness > maximumBrightness)
                        maximumBrightness = brightness;

                    histogram[(int)Math.Clamp(Math.Round(brightness), 0, 255)]++;

                    if (brightness < 40)
                        darkPixels++;

                    if (brightness > 220)
                        brightPixels++;

                    int rq = r / 16;
                    int gq = g / 16;
                    int bq = b / 16;

                    int bucket = (rq << 8) | (gq << 4) | bq;

                    colorBuckets.TryGetValue(bucket, out long count);
                    colorBuckets[bucket] = count + 1;
                }
            }
        });

        var dominantColors = colorBuckets
            .OrderByDescending(x => x.Value)
            .Take(10)
            .Select(x =>
            {
                int rq = (x.Key >> 8) & 0xF;
                int gq = (x.Key >> 4) & 0xF;
                int bq = x.Key & 0xF;

                return new DominantColor
                {
                    Red = rq * 16 + 8,
                    Green = gq * 16 + 8,
                    Blue = bq * 16 + 8,
                    PixelCount = x.Value,
                    Percentage = x.Value * 100.0 / totalPixels
                };
            })
            .ToList();

        return new ImageStatistics
        {
            TotalPixels = totalPixels,

            AverageRed = redSum / (double)totalPixels,
            AverageGreen = greenSum / (double)totalPixels,
            AverageBlue = blueSum / (double)totalPixels,

            AverageBrightness = brightnessSum / totalPixels,
            MinimumBrightness = minimumBrightness,
            MaximumBrightness = maximumBrightness,

            DarkPixelPercentage = darkPixels * 100.0 / totalPixels,
            BrightPixelPercentage = brightPixels * 100.0 / totalPixels,

            BrightnessHistogram = histogram,
            DominantColors = dominantColors
        };
    }
}
