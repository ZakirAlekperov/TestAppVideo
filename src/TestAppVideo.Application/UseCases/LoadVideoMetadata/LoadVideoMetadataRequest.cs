namespace TestAppVideo.Application.UseCases.LoadVideoMetadata;

public sealed class LoadVideoMetadataRequest
{
    public string FilePath { get; }

    public LoadVideoMetadataRequest(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }
}
