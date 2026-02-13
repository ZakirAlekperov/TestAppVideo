using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;

namespace TestAppVideo.Domain.VideoProcessing.Splitting;

public abstract class SplittingStrategy
{
    public abstract bool IsValidFor(Video video);

    public abstract IReadOnlyList<VideoSegment> CalculateSegments(Video video, OutputDirectory outputDirectory);
}
