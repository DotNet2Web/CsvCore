using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Exceptions;

[Serializable]
[ExcludeFromCodeCoverage]
public class DbContextNotSetException : Exception
{
    public DbContextNotSetException() { }

    public DbContextNotSetException(string message) : base(message) { }

    public DbContextNotSetException(string message, Exception inner) : base(message, inner) { }
}
