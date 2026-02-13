namespace TestAppVideo.Application.UseCases.PreviewSplit;

using TestAppVideo.Application.DTOs;

public sealed class PreviewSplitResponse
{
    public bool Success { get; }
    public IReadOnlyList<VideoSegmentDto> Segments { get; }
    public string ErrorMessage { get; }

    private PreviewSplitResponse(bool success, IReadOnlyList<VideoSegmentDto> segments, string errorMessage)
    {
        Success = success;
        Segments = segments;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static PreviewSplitResponse CreateSuccess(IReadOnlyList<VideoSegmentDto> segments) => new(true, segments, string.Empty);
    public static PreviewSplitResponse CreateFailure(string error) => new(false, Array.Empty<VideoSegmentDto>(), error);
}
