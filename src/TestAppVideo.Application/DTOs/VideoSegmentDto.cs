namespace TestAppVideo.Application.DTOs;

public sealed class VideoSegmentDto
{
    public int SegmentNumber { get; init; }
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
    public TimeSpan Duration => End - Start;
    public string OutputFileName { get; init; } = string.Empty;
}
