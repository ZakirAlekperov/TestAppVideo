using TestAppVideo.Domain.Common.Exceptions;

namespace TestAppVideo.Domain.VideoProcessing.Models;

public sealed class VideoMetadata
{
    public TimeSpan Duration { get; }
    public int Width { get; }
    public int Height { get; }
    public string FormatName { get; }
    public string VideoCodec { get; }
    public string AudioCodec { get; }
    public double FrameRate { get; }

    public VideoMetadata(
        TimeSpan duration,
        int width,
        int height,
        string formatName,
        string videoCodec,
        string audioCodec,
        double frameRate)
    {
        if (duration <= TimeSpan.Zero)
            throw new InvalidVideoDurationException("Duration must be positive");
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (string.IsNullOrWhiteSpace(formatName))
            throw new ArgumentException("FormatName cannot be empty", nameof(formatName));

        Duration = duration;
        Width = width;
        Height = height;
        FormatName = formatName;
        VideoCodec = videoCodec ?? string.Empty;
        AudioCodec = audioCodec ?? string.Empty;
        FrameRate = frameRate;
    }
}
