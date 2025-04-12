namespace CsvCore.Reader;

public interface ICsvCoreReader
{
    CsvCoreReader ForFile(string csvFilePath);

    CsvCoreReader UseDelimiter(char customDelimiter);

    CsvCoreReader HasHeaderRecord();

    IEnumerable<T> Read<T>() where T : class;
}
