using System.Globalization;
using CsvCore.Attributes;
using CsvCore.Exceptions;

namespace CsvCore.Writer;

public class CsvCoreWriter : ICsvCoreWriter
{
    private string? delimiter;
    private bool _setHeader = true;

    /// <summary>
    /// Use this method to set a custom delimiter for the CSV file.
    /// </summary>
    /// <param name="customDelimiter"></param>
    /// <returns></returns>
    public CsvCoreWriter UseDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    /// <summary>
    /// Use this method to tell use you don't want to set a header for the CSV file.
    /// </summary>
    /// <returns></returns>
    public CsvCoreWriter WithoutHeader()
    {
        _setHeader = false;
        return this;
    }

    /// <summary>
    /// Use this method to write a collection of models to a CSV file.
    /// </summary>
    /// <param name="filePath">The fullpath were to store the csv file</param>
    /// <param name="records">What should be written as a row in the csv</param>
    /// <typeparam name="T">The model</typeparam>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileWritingException"></exception>
    public void Write<T>(string filePath, IEnumerable<T> records) where T : class
    {
        if (records == null || !records.Any())
        {
            throw new ArgumentException("The records collection cannot be null or empty.");
        }

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        try
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            using var writer = new StreamWriter(filePath);
            var properties = typeof(T).GetProperties();

            if (_setHeader)
            {
                var header = string.Join(delimiter, properties
                    .Select(p => p.GetCustomAttributes(typeof(HeaderAttribute), false)
                        .FirstOrDefault() is HeaderAttribute headerAttribute
                        ? headerAttribute.Name
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
