using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;

namespace TestAppVideo.Domain.VideoProcessing.Ports;

public interface IVideoProcessor
{
    Task<VideoProcessingResult> SplitVideoAsync(
        Video video,
        IReadOnlyList<VideoSegment> segments,
        OutputDirectory outputDirectory,
        IVideoProcessingProgress progress,
        bool useCompression,
        CancellationToken cancellationToken);
}
