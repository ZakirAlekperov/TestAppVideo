namespace TestAppVideo.Application.UseCases.PreviewSplit;

using TestAppVideo.Application.DTOs;

public sealed class PreviewSplitRequest
{
    public string FilePath { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public SplittingParametersDto? SplittingParameters { get; init; }
}
