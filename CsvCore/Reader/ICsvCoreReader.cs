using Microsoft.EntityFrameworkCore;

namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader WithoutHeader();

    CsvCoreReader SetDateTimeFormat(string format);

    CsvCoreReader UseDbContext(DbContext dbContext);

    CsvCoreReader Validate(string? path = null);

    Task<IEnumerable<T>> Read<T>(string filePath) where T : class;

    Task Persist<TEntity>(string filePath) where TEntity : class;
}
