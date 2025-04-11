namespace CsvCore.Exceptions;

public class MissingFileException : Exception
{
    public override string StackTrace { get; } = string.Empty;

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
