namespace CsvCore.Writer;

public interface ICsvCoreWriter
{
    CsvCoreWriter UseDelimiter(char customDelimiter);

    CsvCoreWriter WithoutHeader();

    void Write<T>(string filePath, IEnumerable<T> records) where T : class;
}
