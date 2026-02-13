namespace TestAppVideo.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using TestAppVideo.Application.Services;
using TestAppVideo.Application.UseCases.LoadVideoMetadata;
using TestAppVideo.Application.UseCases.PreviewSplit;
using TestAppVideo.Application.UseCases.SplitVideo;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<LoadVideoMetadataUseCase>();
        services.AddTransient<PreviewSplitUseCase>();
        services.AddTransient<SplitVideoUseCase>();

        services.AddSingleton<IVideoEditorApplicationService, VideoEditorApplicationService>();
        return services;
    }
}
