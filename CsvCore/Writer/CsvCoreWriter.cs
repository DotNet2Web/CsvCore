using System.Globalization;

namespace CsvCore.Writer;

public class CsvCoreWriter : ICsvCoreWriter
{
    private string? delimiter;

    public CsvCoreWriter SetDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    public void Write<T>(string filePath, IEnumerable<T> records) where T : class
    {
        if (records == null || !records.Any())
        {
            throw new ArgumentException("The records collection cannot be null or empty.");
        }

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        using var writer = new StreamWriter(filePath);
        var properties = typeof(T).GetProperties();

        // Write header
        var header = string.Join(delimiter, properties.Select(p => p.Name));
        writer.WriteLine(header);

        // Write records
        foreach (var record in records)
        {
            var values = properties.Select(p => p.GetValue(record)?.ToString() ?? string.Empty);
            var line = string.Join(delimiter, values);
            writer.WriteLine(line);
        }

        writer.Flush();
    }
}
