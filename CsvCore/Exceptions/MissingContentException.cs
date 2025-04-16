using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Exceptions;

[ExcludeFromCodeCoverage]
public class MissingContentException : Exception
{
    public override string StackTrace { get; } = string.Empty;

    public MissingContentException(string message)
        : base(message)
    {
    }

    public MissingContentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MissingContentException()
    {
    }
}
