namespace LearnWithMe.Application.Abstractions;

public sealed class ProfileStorageException(string message, Exception innerException)
    : Exception(message, innerException)
{
}
