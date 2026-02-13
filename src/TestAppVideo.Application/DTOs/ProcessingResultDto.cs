namespace TestAppVideo.Application.DTOs;

public sealed class ProcessingResultDto
{
    public IReadOnlyList<string> OutputFilePaths { get; init; } = Array.Empty<string>();
    public TimeSpan TotalProcessingTime { get; init; }
    public int SegmentsCreated { get; init; }
}
