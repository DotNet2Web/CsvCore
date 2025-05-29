using Microsoft.EntityFrameworkCore;

namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader WithoutHeader();

    CsvCoreReader SetDateTimeFormat(string format);

    CsvCoreReader UseDbContext(DbContext dbContext);

    CsvCoreReader Validate(string? path = null);

    Task<IEnumerable<T>> ReadAsync<T>(string filePath) where T : class;

    IEnumerable<T> Read<T>(string filePath) where T : class;

    Task PersistAsync<TEntity>(string filePath) where TEntity : class;

    void Persist<TEntity>(string filePath) where TEntity : class;
}
