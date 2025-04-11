using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Exceptions;

[ExcludeFromCodeCoverage]
public class MissingFileException : Exception
{
    public MissingFileException(string message)
        : base(message)
    {
    }

    public MissingFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public MissingFileException()
    {
    }
}
