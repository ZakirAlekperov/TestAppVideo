using FFMpegCore;
using TestAppVideo.Domain.Common.Exceptions;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;
using TestAppVideo.Domain.VideoProcessing.Ports;

namespace TestAppVideo.Infrastructure.FFmpeg;

public sealed class FfmpegCoreMetadataReader : IVideoMetadataReader
{
    private readonly FFMpegConfiguration _configuration;

    public FfmpegCoreMetadataReader(FFMpegConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        ConfigureBinaries();
    }

    public async Task<VideoMetadata> ReadMetadataAsync(VideoFilePath filePath, CancellationToken cancellationToken)
    {
        try
        {
            var analysis = await FFProbe.AnalyseAsync(filePath.Value, cancellationToken: cancellationToken);

            var videoStream = analysis.PrimaryVideoStream;
            if (videoStream is null)
                throw new UnsupportedVideoFormatException("No video stream found");

            var audioStream = analysis.PrimaryAudioStream;

            return new VideoMetadata(
                duration: analysis.Duration,
                width: videoStream.Width,
                height: videoStream.Height,
                formatName: analysis.Format.FormatName,
                videoCodec: videoStream.CodecName,
                audioCodec: audioStream?.CodecName ?? "none",
                frameRate: videoStream.FrameRate);
        }
        catch (UnsupportedVideoFormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new VideoMetadataReadException("Failed to read metadata", ex);
        }
    }

    private void ConfigureBinaries()
    {
        if (string.IsNullOrWhiteSpace(_configuration.FFmpegExecutablePath))
            return;

        var folder = Path.GetDirectoryName(_configuration.FFmpegExecutablePath);
        if (string.IsNullOrWhiteSpace(folder))
            return;

        GlobalFFOptions.Configure(options =>
        {
            options.BinaryFolder = folder;
        });
    }
}
