namespace TestAppVideo.Domain.Common.Exceptions;

public sealed class InvalidVideoDurationException : DomainException
{
    public InvalidVideoDurationException(string message) : base(message) { }
}
