using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Diagnostic.Extractor.Models;

namespace UI.Diagnostic.Extractor.Services;

public sealed class WhiteHaloDetector
{
    private const int BrightThreshold = 220;
    private const int DarkThreshold = 80;
    private const int MinimumRegionPixels = 3;

    public WhiteHaloAnalysis Analyze(string filePath)
    {
        using var image = Image.Load<Rgba32>(filePath);

        int width = image.Width;
        int height = image.Height;

        var brightness = new byte[checked(width * height)];
        var candidates = new byte[checked(width * height)];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                int offset = y * width;

                for (int x = 0; x < width; x++)
                {
                    var pixel = row[x];

                    int value = (int)Math.Round(
                        (0.2126 * pixel.R) +
                        (0.7152 * pixel.G) +
                        (0.0722 * pixel.B));

                    brightness[offset + x] =
                        (byte)Math.Clamp(value, 0, 255);
                }
            }
        });

        long candidatePixels = 0;

        for (int y = 0; y < height; y++)
        {
            int offset = y * width;

            for (int x = 0; x < width; x++)
            {
                int index = offset + x;

                if (brightness[index] < BrightThreshold)
                    continue;

                bool nextToDark = false;

                for (int dy = -1; dy <= 1 && !nextToDark; dy++)
                {
                    int ny = y + dy;

                    if (ny < 0 || ny >= height)
                        continue;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = x + dx;

                        if (nx < 0 || nx >= width)
                            continue;

                        if (brightness[(ny * width) + nx] <= DarkThreshold)
                        {
                            nextToDark = true;
                            break;
                        }
                    }
                }

                if (nextToDark)
                {
                    candidates[index] = 1;
                    candidatePixels++;
                }
            }
        }

        var regions = FindRegions(
            candidates,
            width,
            height);

        return new WhiteHaloAnalysis
        {
            BrightThreshold = BrightThreshold,
            DarkThreshold = DarkThreshold,
            CandidatePixels = candidatePixels,
            CandidatePercentage =
                candidatePixels * 100.0 / brightness.Length,
            Regions = regions
        };
    }

    private static List<WhiteHaloRegion> FindRegions(
        byte[] candidates,
        int width,
        int height)
    {
        var regions = new List<WhiteHaloRegion>();
        var visited = new byte[candidates.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int startIndex = y * width + x;

                if (candidates[startIndex] == 0 ||
                    visited[startIndex] != 0)
                    continue;

                var queue = new Queue<int>();

                queue.Enqueue(startIndex);
                visited[startIndex] = 1;

                int pixelCount = 0;

                int minX = x;
                int maxX = x;
                int minY = y;
                int maxY = y;

                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();

                    int currentX = index % width;
                    int currentY = index / width;

                    pixelCount++;

                    minX = Math.Min(minX, currentX);
                    maxX = Math.Max(maxX, currentX);
                    minY = Math.Min(minY, currentY);
                    maxY = Math.Max(maxY, currentY);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = currentY + dy;

                        if (ny < 0 || ny >= height)
                            continue;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = currentX + dx;

                            if (nx < 0 || nx >= width)
                                continue;

                            int neighbor =
                                ny * width + nx;

                            if (candidates[neighbor] == 0 ||
                                visited[neighbor] != 0)
                                continue;

                            visited[neighbor] = 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                if (pixelCount < MinimumRegionPixels)
                    continue;

                int regionWidth = maxX - minX + 1;
                int regionHeight = maxY - minY + 1;

                double density =
                    pixelCount * 100.0 /
                    (regionWidth * regionHeight);

                regions.Add(new WhiteHaloRegion
                {
                    X = minX,
                    Y = minY,
                    Width = regionWidth,
                    Height = regionHeight,
                    PixelCount = pixelCount,
                    Density = density
                });
            }
        }

        return regions
            .OrderByDescending(r => r.PixelCount)
            .Take(50)
            .ToList();
    }
}
