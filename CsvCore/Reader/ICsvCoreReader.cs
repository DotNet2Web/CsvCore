namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader WithoutHeader();

    CsvCoreReader SetDateTimeFormat(string format);

    CsvCoreReader Validate(string? path = null);

    IEnumerable<T> Read<T>(string filePath) where T : class;
}
