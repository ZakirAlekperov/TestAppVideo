namespace TestAppVideo.Application.UseCases.PreviewSplit;

using TestAppVideo.Application.DTOs;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;
using TestAppVideo.Domain.VideoProcessing.Ports;
using TestAppVideo.Domain.VideoProcessing.Splitting;

public sealed class PreviewSplitUseCase
{
    private readonly IVideoMetadataReader _metadataReader;

    public PreviewSplitUseCase(IVideoMetadataReader metadataReader)
    {
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
    }

    public async Task<PreviewSplitResponse> ExecuteAsync(PreviewSplitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return PreviewSplitResponse.CreateFailure("FilePath is required");
            if (request.SplittingParameters is null)
                return PreviewSplitResponse.CreateFailure("SplittingParameters is required");

            var filePath = new VideoFilePath(request.FilePath);
            var metadata = await _metadataReader.ReadMetadataAsync(filePath, cancellationToken);
            var video = new Video(filePath, metadata);

            var outputDir = new OutputDirectory(string.IsNullOrWhiteSpace(request.OutputDirectory)
                ? Path.GetDirectoryName(request.FilePath) ?? string.Empty
                : request.OutputDirectory);

            var strategy = CreateStrategy(request.SplittingParameters);
            if (!strategy.IsValidFor(video))
                return PreviewSplitResponse.CreateFailure("Strategy is not valid for this video");

            var segments = strategy.CalculateSegments(video, outputDir)
                .Select(s => new VideoSegmentDto
                {
                    SegmentNumber = s.SegmentNumber,
                    Start = s.TimeRange.Start,
                    End = s.TimeRange.End,
                    OutputFileName = s.OutputFileName
                })
                .ToList();

            return PreviewSplitResponse.CreateSuccess(segments);
        }
        catch (Exception ex)
        {
            return PreviewSplitResponse.CreateFailure(ex.Message);
        }
    }

    private static SplittingStrategy CreateStrategy(SplittingParametersDto parameters)
    {
        return parameters.Mode switch
        {
            SplittingMode.EqualParts => new EqualPartsSplittingStrategy(parameters.NumberOfParts ?? 0),
            SplittingMode.FixedDuration => new FixedDurationSplittingStrategy(parameters.SegmentDuration ?? TimeSpan.Zero),
            _ => throw new ArgumentOutOfRangeException(nameof(parameters.Mode))
        };
    }
}
