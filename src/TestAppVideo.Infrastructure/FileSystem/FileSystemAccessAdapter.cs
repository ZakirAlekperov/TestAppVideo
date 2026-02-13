namespace TestAppVideo.Infrastructure.FileSystem;

using TestAppVideo.Domain.FileManagement.Ports;

public sealed class FileSystemAccessAdapter : IFileSystemAccess
{
    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken)
        => Task.FromResult(File.Exists(filePath));

    public Task CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        return Task.CompletedTask;
    }

    public Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken)
    {
        var info = new FileInfo(filePath);
        if (!info.Exists)
            throw new FileNotFoundException($"File not found: {filePath}");

        return Task.FromResult(info.Length);
    }
}
