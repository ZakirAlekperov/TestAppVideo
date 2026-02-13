namespace TestAppVideo.Domain.FileManagement.Models;

public sealed class OutputDirectory
{
    public string Value { get; }

    public OutputDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Output directory cannot be empty", nameof(path));

        Value = path;
    }

    public string CombineWithFileName(string fileName) => Path.Combine(Value, fileName);
}
