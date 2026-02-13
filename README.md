# TestAppVideo

Кросс‑платформенное desktop‑приложение (Windows/macOS/Linux) на .NET 8 + Avalonia UI для разрезания видео на равные части или на части фиксированной длительности.

## Архитектура
Clean Architecture: `Presentation -> Application -> Domain <- Infrastructure`.

- **Domain**: модели и правила предметной области (Video, VideoSegment, стратегии разрезания), порты (IVideoProcessor, IVideoMetadataReader, IFileSystemAccess).
- **Application**: use cases (LoadMetadata, PreviewSplit, SplitVideo) и фасад `IVideoEditorApplicationService`.
- **Infrastructure**: адаптеры FFmpeg (через FFMpegCore) и файловой системы.
- **Presentation.Avalonia**: UI (MVVM), DI-композиция.

## Требования
- .NET SDK 8.x
- Установленный `ffmpeg`/`ffprobe` в PATH **или** указание путей в `FFMpegConfiguration` (см. `App.axaml.cs`).

## Запуск
```bash
# restore
 dotnet restore

# run UI
 dotnet run --project src/TestAppVideo.Presentation.Avalonia
```

## Публикация
Примеры (self-contained):

```bash
# Windows
 dotnet publish src/TestAppVideo.Presentation.Avalonia -c Release -r win-x64 --self-contained true -o publish/win-x64

# macOS
 dotnet publish src/TestAppVideo.Presentation.Avalonia -c Release -r osx-x64 --self-contained true -o publish/osx-x64

# Linux
 dotnet publish src/TestAppVideo.Presentation.Avalonia -c Release -r linux-x64 --self-contained true -o publish/linux-x64
```
