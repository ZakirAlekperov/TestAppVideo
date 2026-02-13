using TestAppVideo.Domain.Common.Exceptions;

namespace TestAppVideo.Domain.FileManagement.Models;

public sealed class VideoFilePath
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v"
    };

    public string Value { get; }

    public VideoFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        var extension = Path.GetExtension(path);
        if (!SupportedExtensions.Contains(extension))
            throw new UnsupportedVideoFormatException($"File extension '{extension}' is not supported.");

        Value = path;
    }

    public string FileName => Path.GetFileName(Value);
}
