namespace TestAppVideo.Application.Services;

using TestAppVideo.Application.UseCases.LoadVideoMetadata;
using TestAppVideo.Application.UseCases.PreviewSplit;
using TestAppVideo.Application.UseCases.SplitVideo;
using TestAppVideo.Domain.VideoProcessing.Ports;

public sealed class VideoEditorApplicationService : IVideoEditorApplicationService
{
    private readonly LoadVideoMetadataUseCase _load;
    private readonly PreviewSplitUseCase _preview;
    private readonly SplitVideoUseCase _split;

    public VideoEditorApplicationService(
        LoadVideoMetadataUseCase load,
        PreviewSplitUseCase preview,
        SplitVideoUseCase split)
    {
        _load = load ?? throw new ArgumentNullException(nameof(load));
        _preview = preview ?? throw new ArgumentNullException(nameof(preview));
        _split = split ?? throw new ArgumentNullException(nameof(split));
    }

    public Task<LoadVideoMetadataResponse> LoadVideoMetadataAsync(LoadVideoMetadataRequest request, CancellationToken cancellationToken)
        => _load.ExecuteAsync(request, cancellationToken);

    public Task<PreviewSplitResponse> PreviewSplitAsync(PreviewSplitRequest request, CancellationToken cancellationToken)
        => _preview.ExecuteAsync(request, cancellationToken);

    public Task<SplitVideoResponse> SplitVideoAsync(SplitVideoRequest request, IVideoProcessingProgress progress, CancellationToken cancellationToken)
        => _split.ExecuteAsync(request, progress, cancellationToken);
}
