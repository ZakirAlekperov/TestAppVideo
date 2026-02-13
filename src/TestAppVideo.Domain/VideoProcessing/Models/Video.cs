using TestAppVideo.Domain.FileManagement.Models;

namespace TestAppVideo.Domain.VideoProcessing.Models;

public sealed class Video
{
    public VideoFilePath FilePath { get; }
    public VideoMetadata Metadata { get; }

    public Video(VideoFilePath filePath, VideoMetadata metadata)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public TimeSpan Duration => Metadata.Duration;

    public bool CanBeSplitInto(int numberOfParts)
    {
        if (numberOfParts <= 0)
            return false;

        // Явное правило: минимальный сегмент >= 1 сек.
        var minSegment = TimeSpan.FromSeconds(1);
        return TimeSpan.FromSeconds(Duration.TotalSeconds / numberOfParts) >= minSegment;
    }
}
