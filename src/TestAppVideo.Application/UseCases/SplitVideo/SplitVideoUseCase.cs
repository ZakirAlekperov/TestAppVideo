namespace TestAppVideo.Application.UseCases.SplitVideo;

using TestAppVideo.Application.DTOs;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.FileManagement.Ports;
using TestAppVideo.Domain.VideoProcessing.Models;
using TestAppVideo.Domain.VideoProcessing.Ports;
using TestAppVideo.Domain.VideoProcessing.Splitting;

public sealed class SplitVideoUseCase
{
    private readonly IVideoMetadataReader _metadataReader;
    private readonly IVideoProcessor _processor;
    private readonly IFileSystemAccess _fileSystem;

    public SplitVideoUseCase(IVideoMetadataReader metadataReader, IVideoProcessor processor, IFileSystemAccess fileSystem)
    {
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<SplitVideoResponse> ExecuteAsync(
        SplitVideoRequest request,
        IVideoProcessingProgress progress,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationError = Validate(request);
            if (validationError is not null)
                return SplitVideoResponse.CreateFailure(validationError);

            var exists = await _fileSystem.FileExistsAsync(request.FilePath, cancellationToken);
            if (!exists)
                return SplitVideoResponse.CreateFailure($"File not found: {request.FilePath}");

            var filePath = new VideoFilePath(request.FilePath);
            var metadata = await _metadataReader.ReadMetadataAsync(filePath, cancellationToken);
            var video = new Video(filePath, metadata);

            var outputDir = new OutputDirectory(request.OutputDirectory);
            await _fileSystem.CreateDirectoryAsync(outputDir.Value, cancellationToken);

            var strategy = CreateStrategy(request.SplittingParameters!);
            if (!strategy.IsValidFor(video))
                return SplitVideoResponse.CreateFailure("Strategy is not valid for this video");

            var segments = strategy.CalculateSegments(video, outputDir);

            var result = await _processor.SplitVideoAsync(video, segments, outputDir, progress, request.UseCompression, cancellationToken);
            if (!result.Success)
                return SplitVideoResponse.CreateFailure(result.ErrorMessage);

            var dto = new ProcessingResultDto
            {
                OutputFilePaths = result.OutputFilePaths,
                TotalProcessingTime = result.TotalProcessingTime,
                SegmentsCreated = segments.Count
            };

            return SplitVideoResponse.CreateSuccess(dto);
        }
        catch (OperationCanceledException)
        {
            return SplitVideoResponse.CreateFailure("Operation cancelled");
        }
        catch (Exception ex)
        {
            return SplitVideoResponse.CreateFailure(ex.Message);
        }
    }

    private static string? Validate(SplitVideoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
            return "FilePath is required";
        if (string.IsNullOrWhiteSpace(request.OutputDirectory))
            return "OutputDirectory is required";
        if (request.SplittingParameters is null)
            return "SplittingParameters is required";

        return request.SplittingParameters.Mode switch
        {
            SplittingMode.EqualParts when (request.SplittingParameters.NumberOfParts ?? 0) <= 0
                => "NumberOfParts must be positive",

            SplittingMode.FixedDuration when (request.SplittingParameters.SegmentDuration ?? TimeSpan.Zero) <= TimeSpan.Zero
                => "SegmentDuration must be positive",

            _ => null
        };
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
