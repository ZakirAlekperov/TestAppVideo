namespace TestAppVideo.Application.DTOs;

public sealed class VideoFileDto
{
    public string FilePath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string FormatName { get; init; } = string.Empty;
    public string VideoCodec { get; init; } = string.Empty;
    public string AudioCodec { get; init; } = string.Empty;
    public double FrameRate { get; init; }
    public long FileSizeBytes { get; init; }
}
