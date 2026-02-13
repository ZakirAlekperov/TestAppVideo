namespace TestAppVideo.Domain.Common.Exceptions;

public sealed class VideoMetadataReadException : DomainException
{
    public VideoMetadataReadException(string message, Exception innerException) : base(message, innerException) { }
}
