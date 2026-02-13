namespace TestAppVideo.Domain.FileManagement.Ports;

public interface IFileSystemAccess
{
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken);
    Task CreateDirectoryAsync(string directoryPath, CancellationToken cancellationToken);
    Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken);
}
