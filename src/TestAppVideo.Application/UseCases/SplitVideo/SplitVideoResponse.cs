namespace TestAppVideo.Application.UseCases.SplitVideo;

using TestAppVideo.Application.DTOs;

public sealed class SplitVideoResponse
{
    public bool Success { get; }
    public ProcessingResultDto? Result { get; }
    public string ErrorMessage { get; }

    private SplitVideoResponse(bool success, ProcessingResultDto? result, string errorMessage)
    {
        Success = success;
        Result = result;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static SplitVideoResponse CreateSuccess(ProcessingResultDto result) => new(true, result, string.Empty);
    public static SplitVideoResponse CreateFailure(string error) => new(false, null, error);
}
