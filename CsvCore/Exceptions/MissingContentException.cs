using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Exceptions;

[ExcludeFromCodeCoverage]
public class MissingContentException : Exception
{
    public MissingContentException() { }
    public MissingContentException(string message)
        : base(message)
    {
    }

    public MissingContentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
