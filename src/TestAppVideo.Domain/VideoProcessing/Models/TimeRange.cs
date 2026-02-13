namespace TestAppVideo.Domain.VideoProcessing.Models;

public sealed class TimeRange
{
    public TimeSpan Start { get; }
    public TimeSpan End { get; }

    public TimeRange(TimeSpan start, TimeSpan end)
    {
        if (start < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (end <= start)
            throw new ArgumentException("End must be greater than Start");

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;
}
