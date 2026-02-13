namespace TestAppVideo.Application.DTOs;

public enum SplittingMode
{
    EqualParts = 0,
    FixedDuration = 1
}

public sealed class SplittingParametersDto
{
    public SplittingMode Mode { get; init; }

    public int? NumberOfParts { get; init; }

    public TimeSpan? SegmentDuration { get; init; }
}
