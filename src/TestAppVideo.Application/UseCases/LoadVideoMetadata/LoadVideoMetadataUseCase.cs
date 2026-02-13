namespace TestAppVideo.Application.UseCases.LoadVideoMetadata;

using TestAppVideo.Application.DTOs;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.FileManagement.Ports;
using TestAppVideo.Domain.VideoProcessing.Ports;

public sealed class LoadVideoMetadataUseCase
{
    private readonly IVideoMetadataReader _metadataReader;
    private readonly IFileSystemAccess _fileSystem;

    public LoadVideoMetadataUseCase(IVideoMetadataReader metadataReader, IFileSystemAccess fileSystem)
    {
        _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<LoadVideoMetadataResponse> ExecuteAsync(LoadVideoMetadataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return LoadVideoMetadataResponse.CreateFailure("FilePath is required");

            var exists = await _fileSystem.FileExistsAsync(request.FilePath, cancellationToken);
            if (!exists)
                return LoadVideoMetadataResponse.CreateFailure($"File not found: {request.FilePath}");

            var path = new VideoFilePath(request.FilePath);
            var metadata = await _metadataReader.ReadMetadataAsync(path, cancellationToken);
            var size = await _fileSystem.GetFileSizeAsync(request.FilePath, cancellationToken);

            var dto = new VideoFileDto
            {
                FilePath = path.Value,
                FileName = path.FileName,
                Duration = metadata.Duration,
                Width = metadata.Width,
                Height = metadata.Height,
                FormatName = metadata.FormatName,
                VideoCodec = metadata.VideoCodec,
                AudioCodec = metadata.AudioCodec,
                FrameRate = metadata.FrameRate,
                FileSizeBytes = size
            };

            return LoadVideoMetadataResponse.CreateSuccess(dto);
        }
        catch (Exception ex)
        {
            return LoadVideoMetadataResponse.CreateFailure(ex.Message);
        }
    }
}
