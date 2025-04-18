namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader WithoutHeader();

    IEnumerable<T> Read<T>(string filePath) where T : class;
}
