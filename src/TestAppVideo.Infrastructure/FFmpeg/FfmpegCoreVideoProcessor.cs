using System.Diagnostics;
using FFMpegCore;
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

                await ProcessOneAsync(video.FilePath.Value, outPath, segment, (p) =>
                {
                    progress.ReportProgress(segment.SegmentNumber, segments.Count, p);
                }, cancellationToken);

                outputs.Add(outPath);
                progress.ReportSegmentCompleted(segment.SegmentNumber, outPath);
            }

            sw.Stop();
            return VideoProcessingResult.CreateSuccess(outputs, sw.Elapsed);
        }
        catch (OperationCanceledException)
        {
            return VideoProcessingResult.CreateFailure("Cancelled");
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
        // Явный, предсказуемый сценарий: разрезание без перекодирования (stream copy).
        return FFMpegArguments
            .FromFileInput(inputPath, verifyExists: true, options => options.Seek(segment.TimeRange.Start))
            .OutputToFile(outputPath, overwrite: true, options => options
                .WithDuration(segment.Duration)
                .CopyChannel())
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
