namespace TestAppVideo.Domain.VideoProcessing.Models;

public sealed class VideoSegment
{
    public int SegmentNumber { get; }
    public TimeRange TimeRange { get; }
    public string OutputFileName { get; }

    public VideoSegment(int segmentNumber, TimeRange timeRange, string outputFileName)
    {
        if (segmentNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(segmentNumber));
        if (timeRange is null)
            throw new ArgumentNullException(nameof(timeRange));
        if (string.IsNullOrWhiteSpace(outputFileName))
            throw new ArgumentException("OutputFileName cannot be empty", nameof(outputFileName));

        SegmentNumber = segmentNumber;
        TimeRange = timeRange;
        OutputFileName = outputFileName;
    }

    public TimeSpan Duration => TimeRange.Duration;
}
