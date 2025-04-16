using System.Globalization;
using CsvCore.Exceptions;
using CsvCore.Helpers;
using CsvCore.Models;

namespace CsvCore.Reader;

public class CsvCoreReader : ICsvCoreReader
{
    private string? delimiter;
    private bool hasHeaderRecord;

    public CsvCoreReader UseDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    public CsvCoreReader HasHeaderRecord()
    {
        hasHeaderRecord = true;
        return this;
    }

    public IEnumerable<ValidationModel> IsValid<T>(string filePath)
        where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var validationResults = new List<ValidationModel>();

        var lines = GetContent(filePath);

        var headerItems = new List<string>();

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        if (hasHeaderRecord)
        {
            headerItems = lines[0].Split(delimiter).ToList();
        }

        var records = lines.Skip(hasHeaderRecord ? 1 : 0)
            .Select(l => l.Split(delimiter))
            .ToList();

        var rowNumber = 1;

        var validationHelper = new ValidationHelper();

        foreach (var record in records)
        {
            if (hasHeaderRecord)
            {
                var properties = typeof(T).GetProperties();

                for (var i = 0; i < headerItems.Count; i++)
                {
                    var property = properties.FirstOrDefault(p => p.Name.Equals(headerItems[i], StringComparison.OrdinalIgnoreCase));

                    if (property == null)
                    {
                        continue;
                    }

                    var validationResult = validationHelper.Validate(record[i], property, rowNumber);

                    if(validationResult != null)
                    {
                        validationResults.Add(validationResult);
                    }
                }

                rowNumber++;
            }
        }

        return validationResults;
    }

    public IEnumerable<T> Read<T>(string filePath)
        where T : class
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var lines = GetContent(filePath);

        var headerItems = new List<string>();

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        if (hasHeaderRecord)
        {
            headerItems = lines[0].Split(delimiter).ToList();
        }

        var records = lines.Skip(hasHeaderRecord ? 1 : 0).Select(l => l.Split(delimiter)).ToList();
        if(records.Count == 0)
        {
            throw new MissingContentException($"The file is empty, based on the '{delimiter}' delimiter.");
        }

        var result = Activator.CreateInstance<List<T>>();

        foreach (var record in records)
        {
            var target = Activator.CreateInstance<T>();

            if (hasHeaderRecord)
            {
                GenerateModelBasedOnHeader(headerItems, record, target);
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
            if(property.PropertyType == typeof(DateOnly))
            {
                var date = DateOnly.Parse(record[i], CultureInfo.CurrentCulture);
                property.SetValue(target, date);
                continue;
            }

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

            if(property.PropertyType == typeof(DateOnly))
            {
                var date = DateOnly.Parse(record[i], CultureInfo.CurrentCulture);
                property.SetValue(target, date);
                continue;
            }

            var value = Convert.ChangeType(record[i], property.PropertyType, CultureInfo.CurrentCulture);
            property.SetValue(target, value);
        }
    }

    private List<string> GetContent(string filePath)
    {
        var lines = new List<string>();

        using var reader = new StreamReader(filePath);

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }
}
