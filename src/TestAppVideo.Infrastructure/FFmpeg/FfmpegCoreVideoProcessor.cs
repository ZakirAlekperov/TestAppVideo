using System.Diagnostics;
using FFMpegCore;
using FFMpegCore.Enums;
using TestAppVideo.Domain.FileManagement.Models;
using TestAppVideo.Domain.VideoProcessing.Models;
using TestAppVideo.Domain.VideoProcessing.Ports;

namespace TestAppVideo.Infrastructure.FFmpeg;

public sealed class FfmpegCoreVideoProcessor : IVideoProcessor
{
    private readonly FFMpegConfiguration _configuration;

    public FfmpegCoreVideoProcessor(FFMpegConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        ConfigureBinaries();
    }

    public async Task<VideoProcessingResult> SplitVideoAsync(
        Video video,
        IReadOnlyList<VideoSegment> segments,
        OutputDirectory outputDirectory,
        IVideoProcessingProgress progress,
        bool useCompression,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var outputs = new List<string>(segments.Count);

        try
        {
            for (int i = 0; i < segments.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var segment = segments[i];
                var outPath = outputDirectory.CombineWithFileName(segment.OutputFileName);

                if (useCompression)
                {
                    await ProcessOneWithCompressionAsync(video.FilePath.Value, outPath, segment, (p) =>
                    {
                        progress.ReportProgress(segment.SegmentNumber, segments.Count, p);
                    }, cancellationToken);
                }
                else
                {
                    await ProcessOneAsync(video.FilePath.Value, outPath, segment, (p) =>
                    {
                        progress.ReportProgress(segment.SegmentNumber, segments.Count, p);
                    }, cancellationToken);
                }

                outputs.Add(outPath);
                progress.ReportSegmentCompleted(segment.SegmentNumber, outPath);
            }

            sw.Stop();
            return VideoProcessingResult.CreateSuccess(outputs, sw.Elapsed);
        }
        catch (OperationCanceledException)
        {
            return VideoProcessingResult.CreateFailure("Отменено");
        }
        catch (Exception ex)
        {
            return VideoProcessingResult.CreateFailure(ex.Message);
        }
    }

    private static Task ProcessOneAsync(
        string inputPath,
        string outputPath,
        VideoSegment segment,
        Action<double> onProgress,
        CancellationToken cancellationToken)
    {
        // Быстрый режим: stream copy без перекодирования
        return FFMpegArguments
            .FromFileInput(inputPath, verifyExists: true, options => options.Seek(segment.TimeRange.Start))
            .OutputToFile(outputPath, overwrite: true, options => options
                .WithDuration(segment.Duration)
                .CopyChannel())
            .NotifyOnProgress(onProgress, segment.Duration)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();
    }

    private static Task ProcessOneWithCompressionAsync(
        string inputPath,
        string outputPath,
        VideoSegment segment,
        Action<double> onProgress,
        CancellationToken cancellationToken)
    {
        // Сжатие для соцсетей: H.264, CRF 23, пресет medium
        return FFMpegArguments
            .FromFileInput(inputPath, verifyExists: true, options => options.Seek(segment.TimeRange.Start))
            .OutputToFile(outputPath, overwrite: true, options => options
                .WithDuration(segment.Duration)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(23)
                .WithSpeedPreset(Speed.Medium)
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(128))
            .NotifyOnProgress(onProgress, segment.Duration)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();
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
