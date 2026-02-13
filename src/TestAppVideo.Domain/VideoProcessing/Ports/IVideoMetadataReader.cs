using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;

namespace TestAppVideo.Domain.VideoProcessing.Ports;

public interface IVideoMetadataReader
{
    Task<VideoMetadata> ReadMetadataAsync(VideoFilePath filePath, CancellationToken cancellationToken);
}
