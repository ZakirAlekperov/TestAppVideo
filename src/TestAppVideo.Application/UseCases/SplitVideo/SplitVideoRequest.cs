namespace TestAppVideo.Application.UseCases.SplitVideo;

using TestAppVideo.Application.DTOs;

public sealed class SplitVideoRequest
{
    public string FilePath { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public SplittingParametersDto? SplittingParameters { get; init; }
}
