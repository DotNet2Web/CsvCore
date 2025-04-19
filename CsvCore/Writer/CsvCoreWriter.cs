using System.Globalization;
using CsvCore.Attributes;
using CsvCore.Exceptions;

namespace CsvCore.Writer;

public class CsvCoreWriter : ICsvCoreWriter
{
    private string? delimiter;
    private bool _setHeader = true;

    public CsvCoreWriter UseDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    public CsvCoreWriter WithoutHeader()
    {
        _setHeader = false;
        return this;
    }

    public void Write<T>(string filePath, IEnumerable<T> records) where T : class
    {
        if (records == null || !records.Any())
        {
            throw new ArgumentException("The records collection cannot be null or empty.");
        }

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        try
        {
            using var writer = new StreamWriter(filePath);
            var properties = typeof(T).GetProperties();

            if (_setHeader)
            {
                var header = string.Join(delimiter, properties
                    .Select(p => p.GetCustomAttributes(typeof(HeaderAttribute), false)
                        .FirstOrDefault() is HeaderAttribute headerAttribute
                        ? headerAttribute.Value
                        : p.Name));

                writer.WriteLine(header);
            }

            foreach (var record in records)
            {
                var values = properties.Select(p => p.GetValue(record)?.ToString() ?? string.Empty);
                var line = string.Join(delimiter, values);
                writer.WriteLine(line);
            }

            writer.Flush();
        }
        catch (Exception e)
        {
            throw new FileWritingException($"Could not write the CSV file to {filePath}, please check the exception.", e);
        }
    }
}
