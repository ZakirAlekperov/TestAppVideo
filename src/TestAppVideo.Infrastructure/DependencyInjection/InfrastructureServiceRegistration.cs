namespace TestAppVideo.Infrastructure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using TestAppVideo.Domain.FileManagement.Ports;
using TestAppVideo.Domain.VideoProcessing.Ports;
using TestAppVideo.Infrastructure.FFmpeg;
using TestAppVideo.Infrastructure.FileSystem;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, FFMpegConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddSingleton<IFileSystemAccess, FileSystemAccessAdapter>();
        services.AddSingleton<IVideoMetadataReader, FfmpegCoreMetadataReader>();
        services.AddSingleton<IVideoProcessor, FfmpegCoreVideoProcessor>();

        return services;
    }
}
