namespace TestAppVideo.Infrastructure.FFmpeg;

public sealed class FFMpegConfiguration
{
    public string? FFmpegExecutablePath { get; set; }
    public string? FFprobeExecutablePath { get; set; }

    // Задел под будущее: параллельная обработка сегментов.
    public int MaxDegreeOfParallelism { get; set; } = 1;
}
