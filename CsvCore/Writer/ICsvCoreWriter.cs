namespace CsvCore.Writer;

public interface ICsvCoreWriter
{
    CsvCoreWriter SetDelimiter(char customDelimiter);

    void Write<T>(string filePath, IEnumerable<T> records) where T : class;
}
