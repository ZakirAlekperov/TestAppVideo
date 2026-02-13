using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TestAppVideo.Application.DTOs;
using TestAppVideo.Application.Services;
using TestAppVideo.Application.UseCases.LoadVideoMetadata;
using TestAppVideo.Application.UseCases.PreviewSplit;
using TestAppVideo.Application.UseCases.SplitVideo;
using TestAppVideo.Domain.VideoProcessing.Ports;

namespace TestAppVideo.Presentation.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IVideoEditorApplicationService _app;
    private Window? _window;

    private string _inputFilePath = string.Empty;
    public string InputFilePath
    {
        get => _inputFilePath;
        set => SetProperty(ref _inputFilePath, value);
    }

    private string _outputDirectory = string.Empty;
    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    private int _numberOfParts = 4;
    public int NumberOfParts
    {
        get => _numberOfParts;
        set => SetProperty(ref _numberOfParts, value);
    }

    private int _codecModeIndex = 0;
    public int CodecModeIndex
    {
        get => _codecModeIndex;
        set => SetProperty(ref _codecModeIndex, value);
    }

    private string _status = "Готов к работе";
    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        private set => SetProperty(ref _progress, value);
    }

    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (SetProperty(ref _isProcessing, value))
            {
                ((AsyncCommand)LoadMetadataCommand).RaiseCanExecuteChanged();
                ((AsyncCommand)PreviewCommand).RaiseCanExecuteChanged();
                ((AsyncCommand)SplitCommand).RaiseCanExecuteChanged();
                ((Command)CancelCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private VideoFileDto? _loadedVideo;
    public VideoFileDto? LoadedVideo
    {
        get => _loadedVideo;
        private set
        {
            if (SetProperty(ref _loadedVideo, value))
            {
                ((AsyncCommand)PreviewCommand).RaiseCanExecuteChanged();
                ((AsyncCommand)SplitCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<VideoSegmentDto> PreviewSegments { get; } = new();

    public ICommand BrowseInputFileCommand { get; }
    public ICommand BrowseOutputFolderCommand { get; }
    public ICommand LoadMetadataCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand SplitCommand { get; }
    public ICommand CancelCommand { get; }

    private CancellationTokenSource? _cts;

    public MainWindowViewModel(IVideoEditorApplicationService app)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));

        BrowseInputFileCommand = new AsyncCommand(BrowseInputFileAsync, () => !IsProcessing);
        BrowseOutputFolderCommand = new AsyncCommand(BrowseOutputFolderAsync, () => !IsProcessing);
        LoadMetadataCommand = new AsyncCommand(LoadMetadataAsync, () => !IsProcessing);
        PreviewCommand = new AsyncCommand(PreviewAsync, () => !IsProcessing && LoadedVideo is not null);
        SplitCommand = new AsyncCommand(SplitAsync, () => !IsProcessing && LoadedVideo is not null);
        CancelCommand = new Command(Cancel, () => IsProcessing);
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    private async Task BrowseInputFileAsync()
    {
        if (_window is null) return;

        var storageProvider = _window.StorageProvider;
        if (!storageProvider.CanOpen) return;

        var options = new FilePickerOpenOptions
        {
            Title = "Выберите видеофайл",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Видеофайлы")
                {
                    Patterns = new[] { "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv", "*.flv", "*.webm" },
                    MimeTypes = new[] { "video/*" }
                },
                new FilePickerFileType("Все файлы")
                {
                    Patterns = new[] { "*.*" }
                }
            }
        };

        var result = await storageProvider.OpenFilePickerAsync(options);
        if (result.Count > 0)
        {
            InputFilePath = result[0].Path.LocalPath;
            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                OutputDirectory = Path.GetDirectoryName(InputFilePath) ?? string.Empty;
            }
        }
    }

    private async Task BrowseOutputFolderAsync()
    {
        if (_window is null) return;

        var storageProvider = _window.StorageProvider;
        if (!storageProvider.CanPickFolder) return;

        var options = new FolderPickerOpenOptions
        {
            Title = "Выберите папку для сохранения",
            AllowMultiple = false
        };

        var result = await storageProvider.OpenFolderPickerAsync(options);
        if (result.Count > 0)
        {
            OutputDirectory = result[0].Path.LocalPath;
        }
    }

    private async Task LoadMetadataAsync()
    {
        Status = "Загрузка метаданных...";
        Progress = 0;

        var response = await _app.LoadVideoMetadataAsync(new LoadVideoMetadataRequest(InputFilePath), CancellationToken.None);
        if (!response.Success)
        {
            LoadedVideo = null;
            Status = response.ErrorMessage;
            return;
        }

        LoadedVideo = response.VideoFile;
        Status = $"Загружено: {LoadedVideo!.FileName} ({LoadedVideo.Duration})";

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            OutputDirectory = Path.GetDirectoryName(InputFilePath) ?? string.Empty;

        PreviewSegments.Clear();
    }

    private async Task PreviewAsync()
    {
        Status = "Расчёт предпросмотра...";

        var parameters = BuildParameters();
        var response = await _app.PreviewSplitAsync(new PreviewSplitRequest
        {
            FilePath = InputFilePath,
            OutputDirectory = OutputDirectory,
            SplittingParameters = parameters
        }, CancellationToken.None);

        if (!response.Success)
        {
            Status = response.ErrorMessage;
            return;
        }

        PreviewSegments.Clear();
        foreach (var s in response.Segments)
            PreviewSegments.Add(s);

        Status = $"Предпросмотр: {PreviewSegments.Count} сегментов";
    }

    private async Task SplitAsync()
    {
        IsProcessing = true;
        Progress = 0;
        Status = "Обработка...";

        _cts = new CancellationTokenSource();

        try
        {
            var parameters = BuildParameters();

            var progress = new UiProgress(
                onProgress: (current, total, percent) =>
                {
                    Status = $"Сегмент {current}/{total} ({percent:0.0}%)";
                    Progress = ((current - 1) * 100.0 + percent) / total;
                },
                onCompleted: (segment, path) => { },
                onError: (segment, ex) => { Status = $"Ошибка в сегменте {segment}: {ex.Message}"; }
            );

            var response = await _app.SplitVideoAsync(new SplitVideoRequest
            {
                FilePath = InputFilePath,
                OutputDirectory = OutputDirectory,
                SplittingParameters = parameters,
                UseCompression = CodecModeIndex == 1
            }, progress, _cts.Token);

            if (!response.Success)
            {
                Status = response.ErrorMessage;
                return;
            }

            Progress = 100;
            Status = $"Готово! Файлов: {response.Result!.SegmentsCreated}, время: {response.Result.TotalProcessingTime}";
        }
        finally
        {
            IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private SplittingParametersDto BuildParameters()
    {
        return new SplittingParametersDto
        {
            Mode = SplittingMode.EqualParts,
            NumberOfParts = NumberOfParts
        };
    }

    private sealed class UiProgress : IVideoProcessingProgress
    {
        private readonly Action<int, int, double> _onProgress;
        private readonly Action<int, string> _onCompleted;
        private readonly Action<int, Exception> _onError;

        public UiProgress(Action<int, int, double> onProgress, Action<int, string> onCompleted, Action<int, Exception> onError)
        {
            _onProgress = onProgress;
            _onCompleted = onCompleted;
            _onError = onError;
        }

        public void ReportProgress(int currentSegment, int totalSegments, double percentComplete)
            => _onProgress(currentSegment, totalSegments, percentComplete);

        public void ReportSegmentCompleted(int segmentNumber, string outputFilePath)
            => _onCompleted(segmentNumber, outputFilePath);

        public void ReportSegmentError(int segmentNumber, Exception error)
            => _onError(segmentNumber, error);
    }

    private sealed class Command : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler? CanExecuteChanged;

        public Command(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute();

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler? CanExecuteChanged;

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute();

        public async void Execute(object? parameter) => await _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
