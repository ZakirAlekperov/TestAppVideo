namespace TestAppVideo.Domain.VideoProcessing.Models;

public sealed class VideoProcessingResult
{
    public bool Success { get; }
    public IReadOnlyList<string> OutputFilePaths { get; }
    public TimeSpan TotalProcessingTime { get; }
    public string ErrorMessage { get; }

    private VideoProcessingResult(bool success, IReadOnlyList<string> outputFilePaths, TimeSpan totalProcessingTime, string errorMessage)
    {
        Success = success;
        OutputFilePaths = outputFilePaths;
        TotalProcessingTime = totalProcessingTime;
        ErrorMessage = errorMessage;
    }

    public static VideoProcessingResult CreateSuccess(IReadOnlyList<string> outputFilePaths, TimeSpan totalProcessingTime)
        => new(true, outputFilePaths ?? Array.Empty<string>(), totalProcessingTime, string.Empty);

    public static VideoProcessingResult CreateFailure(string errorMessage)
        => new(false, Array.Empty<string>(), TimeSpan.Zero, errorMessage ?? "Unknown error");
}
