using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;

namespace TestAppVideo.Domain.VideoProcessing.Splitting;

public sealed class EqualPartsSplittingStrategy : SplittingStrategy
{
    public int NumberOfParts { get; }

    public EqualPartsSplittingStrategy(int numberOfParts)
    {
        if (numberOfParts <= 0)
            throw new ArgumentOutOfRangeException(nameof(numberOfParts));

        NumberOfParts = numberOfParts;
    }

    public override bool IsValidFor(Video video) => video.CanBeSplitInto(NumberOfParts);

    public override IReadOnlyList<VideoSegment> CalculateSegments(Video video, OutputDirectory outputDirectory)
    {
        if (!IsValidFor(video))
            throw new InvalidOperationException("Strategy is not valid for this video");

        var segmentSeconds = video.Duration.TotalSeconds / NumberOfParts;
        var segments = new List<VideoSegment>(NumberOfParts);

        for (int i = 0; i < NumberOfParts; i++)
        {
            var start = TimeSpan.FromSeconds(i * segmentSeconds);
            var end = (i == NumberOfParts - 1)
                ? video.Duration
                : TimeSpan.FromSeconds((i + 1) * segmentSeconds);

            var fileName = BuildFileName(video.FilePath.Value, i + 1, NumberOfParts);
            segments.Add(new VideoSegment(i + 1, new TimeRange(start, end), fileName));
        }

        return segments;
    }

    private static string BuildFileName(string inputPath, int partNumber, int total)
    {
        var name = Path.GetFileNameWithoutExtension(inputPath);
        var ext = Path.GetExtension(inputPath);
        return $"{name}_part{partNumber:D3}_of_{total:D3}{ext}";
    }
}
