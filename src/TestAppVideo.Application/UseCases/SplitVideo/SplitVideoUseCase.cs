using TestAppVideo.Application.DTOs;
using TestAppVideo.Domain.Common.Results;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;
using TestAppVideo.Domain.VideoProcessing.Ports;
using TestAppVideo.Domain.VideoProcessing.Services;

namespace TestAppVideo.Application.UseCases.SplitVideo;

public sealed class SplitVideoUseCase
{
    private readonly IVideoMetadataReader _metadataReader;
    private readonly IVideoProcessor _videoProcessor;
    private readonly IFileSystemService _fileSystem;

    public SplitVideoUseCase(
        IVideoMetadataReader metadataReader,
        IVideoProcessor videoProcessor,
        IFileSystemService fileSystem)
    {
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
        _videoProcessor = videoProcessor ?? throw new ArgumentNullException(nameof(videoProcessor));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<Result<SplitVideoResult>> ExecuteAsync(
        SplitVideoRequest request,
        IVideoProcessingProgress progress,
        CancellationToken cancellationToken)
    {
        var filePathResult = VideoFilePath.Create(request.FilePath);
        if (!filePathResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(filePathResult.Error);

        var outputDirResult = OutputDirectory.Create(request.OutputDirectory);
        if (!outputDirResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(outputDirResult.Error);

        if (!_fileSystem.FileExists(filePathResult.Value.Value))
            return Result<SplitVideoResult>.Failure("Файл не найден");

        if (!_fileSystem.DirectoryExists(outputDirResult.Value.Value))
            _fileSystem.CreateDirectory(outputDirResult.Value.Value);

        var metadata = await _metadataReader.ReadMetadataAsync(filePathResult.Value, cancellationToken);
        var videoResult = Video.Create(filePathResult.Value, metadata);
        if (!videoResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(videoResult.Error);

        var parametersResult = ParseParameters(request.SplittingParameters);
        if (!parametersResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(parametersResult.Error);

        var splitter = new VideoSplitter();
        var segmentsResult = splitter.CalculateSegments(videoResult.Value, parametersResult.Value, outputDirResult.Value);
        if (!segmentsResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(segmentsResult.Error);

        var processingResult = await _videoProcessor.SplitVideoAsync(
            videoResult.Value,
            segmentsResult.Value,
            outputDirResult.Value,
            progress,
            request.UseCompression,
            cancellationToken);

        if (!processingResult.IsSuccess)
            return Result<SplitVideoResult>.Failure(processingResult.ErrorMessage);

        return Result<SplitVideoResult>.Success(new SplitVideoResult
        {
            SegmentsCreated = processingResult.OutputFiles.Count,
            TotalProcessingTime = processingResult.ProcessingTime.ToString(@"hh\:mm\:ss")
        });
    }

    private static Result<SplittingParameters> ParseParameters(SplittingParametersDto dto)
    {
        if (dto.Mode == SplittingMode.EqualParts)
        {
            if (dto.NumberOfParts is null || dto.NumberOfParts < 2)
                return Result<SplittingParameters>.Failure("Укажите корректное количество частей (>= 2)");

            return Result<SplittingParameters>.Success(SplittingParameters.CreateEqualParts(dto.NumberOfParts.Value));
        }

        if (dto.Mode == SplittingMode.FixedDuration)
        {
            if (dto.SegmentDuration is null || dto.SegmentDuration <= TimeSpan.Zero)
                return Result<SplittingParameters>.Failure("Укажите корректную длительность сегмента");

            return Result<SplittingParameters>.Success(SplittingParameters.CreateFixedDuration(dto.SegmentDuration.Value));
        }

        return Result<SplittingParameters>.Failure("Неизвестный режим разделения");
    }
}
