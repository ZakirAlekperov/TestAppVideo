namespace TestAppVideo.Application.Services;

using TestAppVideo.Application.UseCases.LoadVideoMetadata;
using TestAppVideo.Application.UseCases.PreviewSplit;
using TestAppVideo.Application.UseCases.SplitVideo;
using TestAppVideo.Domain.VideoProcessing.Ports;

public interface IVideoEditorApplicationService
{
    Task<LoadVideoMetadataResponse> LoadVideoMetadataAsync(LoadVideoMetadataRequest request, CancellationToken cancellationToken);

    Task<PreviewSplitResponse> PreviewSplitAsync(PreviewSplitRequest request, CancellationToken cancellationToken);

    Task<SplitVideoResponse> SplitVideoAsync(SplitVideoRequest request, IVideoProcessingProgress progress, CancellationToken cancellationToken);
}
