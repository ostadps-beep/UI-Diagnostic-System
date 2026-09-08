using System.Text.Json;
using UI.Diagnostic.Extractor.Services;

var defaultImagePath = Path.GetFullPath(
    Path.Combine(
        "diagnostics",
        "screenshots",
        "001.png"));

var imagePath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : defaultImagePath;

Console.WriteLine("UI Diagnostic Extractor");
Console.WriteLine("-----------------------");
Console.WriteLine($"Image: {imagePath}");

try
{
    var loader = new ImageLoader();
    var pixelAnalyzer = new PixelAnalyzer();
    var whiteHaloDetector = new WhiteHaloDetector();
    var edgeAnalyzer = new EdgeAnalyzer();

    var imageInfo = loader.Load(imagePath);
    var statistics = pixelAnalyzer.Analyze(imagePath);
    var whiteHalo = whiteHaloDetector.Analyze(imagePath);

    Console.WriteLine();
    Console.WriteLine($"File Name : {imageInfo.FileName}");
    Console.WriteLine($"File Size : {imageInfo.FileSizeBytes:N0} bytes");
    Console.WriteLine($"Dimensions: {imageInfo.Width} x {imageInfo.Height}");
    Console.WriteLine($"Format    : {imageInfo.Format}");

    Console.WriteLine();
    Console.WriteLine("Pixel Statistics");
    Console.WriteLine("----------------");
    Console.WriteLine($"Total Pixels       : {statistics.TotalPixels:N0}");
    Console.WriteLine($"Average Brightness : {statistics.AverageBrightness:F2}");
    Console.WriteLine($"Minimum Brightness : {statistics.MinimumBrightness:F2}");
    Console.WriteLine($"Maximum Brightness : {statistics.MaximumBrightness:F2}");
    Console.WriteLine($"Dark Pixels        : {statistics.DarkPixelPercentage:F2}%");
    Console.WriteLine($"Bright Pixels      : {statistics.BrightPixelPercentage:F2}%");

    Console.WriteLine();
    Console.WriteLine("White Halo Candidates");
    Console.WriteLine("----------------------");
    Console.WriteLine($"Candidate Pixels   : {whiteHalo.CandidatePixels:N0}");
    Console.WriteLine($"Candidate Area     : {whiteHalo.CandidatePercentage:F4}%");
    Console.WriteLine($"Candidate Regions  : {whiteHalo.Regions.Count}");

    Console.WriteLine();
    Console.WriteLine("Edge / Transition Analysis");
    Console.WriteLine("---------------------------");

    var edges = edgeAnalyzer.Analyze(imagePath);

    Console.WriteLine($"Horizontal Transitions : {edges.HorizontalTransitions:N0}");
    Console.WriteLine($"Vertical Transitions   : {edges.VerticalTransitions:N0}");
    Console.WriteLine($"Reported Edge Regions  : {edges.Regions.Count}");

    if (edges.Regions.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Longest Edge Regions:");

        foreach (var region in edges.Regions.Take(15))
        {
            Console.WriteLine(
                $"{region.Orientation,-10} " +
                $"X={region.X,5} Y={region.Y,5} " +
                $"W={region.Width,5} H={region.Height,5} " +
                $"Length={region.Length,5} " +
                $"?Brightness={region.BrightnessDifference,3}");
        }
    }

    var diagnostic = new
    {
        Image = imageInfo,
        Statistics = statistics,
        WhiteHalo = whiteHalo,
        Edges = edges
    };

    var outputDirectory = Path.GetDirectoryName(imagePath)!;

    var jsonPath = Path.Combine(
        outputDirectory,
        $"{Path.GetFileNameWithoutExtension(imagePath)}.diagnostic.json");

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    File.WriteAllText(
        jsonPath,
        JsonSerializer.Serialize(diagnostic, jsonOptions));

    Console.WriteLine();
    Console.WriteLine("Diagnostic JSON created:");
    Console.WriteLine(jsonPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    Environment.ExitCode = 1;
}
