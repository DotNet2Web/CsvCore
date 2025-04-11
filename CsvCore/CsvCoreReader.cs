using System.Globalization;
using CsvCore.Exceptions;

namespace CsvCore;

public class CsvCoreReader
{
    private string? filePath;
    private char delimiter;
    private bool hasHeaderRecord;

    public CsvCoreReader ForFile(string csvFilePath)
    {
        filePath = csvFilePath;
        return this;
    }

    public CsvCoreReader UseDelimiter(char delimiterCharacter)
    {
        delimiter = delimiterCharacter;
        return this;
    }

    public CsvCoreReader HasHeaderRecord()
    {
        hasHeaderRecord = true;
        return this;
    }

    public IEnumerable<T> Read<T>()
        where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var lines = GetContent();

        var header = new List<string>();

        if (hasHeaderRecord)
        {
            header = lines[0].Split(delimiter).ToList();
        }


        var records = lines.Skip(hasHeaderRecord ? 1 : 0).Select(l => l.Split(delimiter)).ToList();

        var result = Activator.CreateInstance<List<T>>();

        foreach (var record in records)
        {
            var target = Activator.CreateInstance<T>();

            if (hasHeaderRecord)
            {
                GenerateModelBasedOnHeader(header, record, target);
            }
            else
            {
                GenerateModel(record, target);
            }

            result.Add(target);
        }

        return result;
    }

    private static void GenerateModel<T>(string[] record, T target)
        where T : class
    {
        var properties = typeof(T).GetProperties();

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var value = Convert.ChangeType(record[i], property.PropertyType, CultureInfo.InvariantCulture);
            property.SetValue(target, value);
        }
    }

    private static void GenerateModelBasedOnHeader<T>(List<string> header, string[] record, T target)
        where T : class
    {
        var properties = typeof(T).GetProperties();

        for (var i = 0; i < header.Count; i++)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(header[i], StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                continue;
            }

            var value = Convert.ChangeType(record[i], property.PropertyType, CultureInfo.InvariantCulture);
            property.SetValue(target, value);
        }
    }

    private List<string> GetContent()
    {
        var lines = new List<string>();

        using var reader = new StreamReader(filePath!);

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }
}
