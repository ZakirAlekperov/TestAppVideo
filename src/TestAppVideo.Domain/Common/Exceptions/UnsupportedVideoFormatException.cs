namespace TestAppVideo.Domain.Common.Exceptions;

public sealed class UnsupportedVideoFormatException : DomainException
{
    public UnsupportedVideoFormatException(string message) : base(message) { }
}
