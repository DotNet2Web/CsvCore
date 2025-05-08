namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader WithoutHeader();

    CsvCoreReader SetErrorPath(string? errorPath = null);

    CsvCoreReader SetDateTimeFormat(string format);

    CsvCoreReader SkipValidation();

    IEnumerable<T> Read<T>(string filePath) where T : class;
}
