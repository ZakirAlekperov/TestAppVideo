namespace TestAppVideo.Application.UseCases.LoadVideoMetadata;

using TestAppVideo.Application.DTOs;

public sealed class LoadVideoMetadataResponse
{
    public bool Success { get; }
    public VideoFileDto? VideoFile { get; }
    public string ErrorMessage { get; }

    private LoadVideoMetadataResponse(bool success, VideoFileDto? videoFile, string errorMessage)
    {
        Success = success;
        VideoFile = videoFile;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static LoadVideoMetadataResponse CreateSuccess(VideoFileDto dto) => new(true, dto, string.Empty);

    public static LoadVideoMetadataResponse CreateFailure(string error) => new(false, null, error);
}
