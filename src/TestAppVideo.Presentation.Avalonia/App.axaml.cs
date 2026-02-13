using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TestAppVideo.Application.DependencyInjection;
using TestAppVideo.Infrastructure.DependencyInjection;
using TestAppVideo.Infrastructure.FFmpeg;
using TestAppVideo.Presentation.Avalonia.ViewModels;
using TestAppVideo.Presentation.Avalonia.Views;

namespace TestAppVideo.Presentation.Avalonia;

public sealed class App : Avalonia.Application
{
    private ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        _services = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = _services!.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = vm };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Явная конфигурация FFmpeg.
        // По умолчанию предполагается, что ffmpeg/ffprobe доступны в PATH.
        var ffmpegConfig = new FFMpegConfiguration
        {
            FFmpegExecutablePath = null,
            FFprobeExecutablePath = null,
            MaxDegreeOfParallelism = 1
        };

        services.AddInfrastructure(ffmpegConfig);
        services.AddApplication();

        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
