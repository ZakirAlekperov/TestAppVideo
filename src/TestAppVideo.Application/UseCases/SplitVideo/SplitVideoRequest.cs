namespace TestAppVideo.Application.UseCases.SplitVideo;

public sealed record SplitVideoRequest
{
    public required string FilePath { get; init; }
    public required string OutputDirectory { get; init; }
    public required SplittingParametersDto SplittingParameters { get; init; }
    public bool UseCompression { get; init; } = false;
}
