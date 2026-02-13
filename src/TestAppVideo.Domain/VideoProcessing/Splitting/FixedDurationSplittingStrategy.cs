using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;

namespace TestAppVideo.Domain.VideoProcessing.Splitting;

public sealed class FixedDurationSplittingStrategy : SplittingStrategy
{
    public TimeSpan SegmentDuration { get; }

    public FixedDurationSplittingStrategy(TimeSpan segmentDuration)
    {
        if (segmentDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(segmentDuration));

        SegmentDuration = segmentDuration;
    }

    public override bool IsValidFor(Video video) => SegmentDuration <= video.Duration;

    public override IReadOnlyList<VideoSegment> CalculateSegments(Video video, OutputDirectory outputDirectory)
    {
        if (!IsValidFor(video))
            throw new InvalidOperationException("Strategy is not valid for this video");

        var segments = new List<VideoSegment>();

        var start = TimeSpan.Zero;
        var index = 1;

        while (start < video.Duration)
        {
            var end = start + SegmentDuration;
            if (end > video.Duration)
                end = video.Duration;

            var fileName = BuildFileName(video.FilePath.Value, index);
            segments.Add(new VideoSegment(index, new TimeRange(start, end), fileName));

            start = end;
            index++;
        }

        return segments;
    }

    private static string BuildFileName(string inputPath, int partNumber)
    {
        var name = Path.GetFileNameWithoutExtension(inputPath);
        var ext = Path.GetExtension(inputPath);
        return $"{name}_part{partNumber:D3}{ext}";
    }
}
