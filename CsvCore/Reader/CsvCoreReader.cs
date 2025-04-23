using System.Globalization;
using System.Reflection;
using CsvCore.Attributes;
using CsvCore.Exceptions;
using CsvCore.Helpers;
using CsvCore.Models;
using CsvCore.Writer;

namespace CsvCore.Reader;

public class CsvCoreReader : ICsvCoreReader
{
    private string? delimiter;
    private bool hasHeaderRecord = true;
    private string errorFolderPath = string.Empty;

    public CsvCoreReader UseDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    public CsvCoreReader WithoutHeader()
    {
        hasHeaderRecord = false;
        return this;
    }

    public CsvCoreReader WriteErrorsAt(string? errorPath = null)
    {
        errorFolderPath = string.IsNullOrEmpty(errorPath) ?
            Path.Combine(Directory.GetCurrentDirectory(), "Errors") :
            errorPath;

        if (!Directory.Exists(errorFolderPath))
        {
            Directory.CreateDirectory(errorFolderPath);
        }

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
                    var property =
                        properties.FirstOrDefault(p => p.Name.Equals(headerItems[i], StringComparison.OrdinalIgnoreCase));

                    if (property == null)
                    {
                        continue;
                    }

                    var validationResult = validationHelper.Validate(record[i], property, rowNumber);

                    if (validationResult != null)
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

        var records = lines.Skip(hasHeaderRecord ? 1 : 0)
            .Select(line => line.Split(delimiter))
            .ToList();

        var result = Activator.CreateInstance<List<T>>();
        var rowNumber = 1;

        var validationResults = new List<ValidationModel>();

        foreach (var record in records)
        {
            var target = Activator.CreateInstance<T>();

            validationResults.AddRange(hasHeaderRecord
                ? GenerateModelBasedOnHeader(headerItems, record, target, rowNumber)
                : GenerateModel(record, target, rowNumber));

            if (validationResults.Any())
            {
                continue;
            }

            result.Add(target);

            rowNumber++;
        }

        if (!validationResults.Any())
        {
            return result;
        }

        var errorFile = Path.GetFileNameWithoutExtension(filePath);

        new CsvCoreWriter()
            .Write(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"), validationResults);

        return result;
    }

    private static IEnumerable<ValidationModel> GenerateModelBasedOnHeader<T>(List<string> header, string[] record, T target,
        int rowNumber)
        where T : class
    {
        var properties = typeof(T).GetProperties();
        var validationHelper = new ValidationHelper();
        var validationResults = new List<ValidationModel>();

        for (var i = 0; i < header.Count; i++)
        {
            var property = GetProperty(header, properties, i);

            if (property == null)
            {
                continue;
            }

            var validationResult = validationHelper.Validate(record[i], property, rowNumber);

            if (validationResult != null)
            {
                validationResults.Add(validationResult);
                continue;
            }

            if (property.PropertyType == typeof(DateOnly))
            {
                var date = DateOnly.Parse(record[i], CultureInfo.CurrentCulture);
                property.SetValue(target, date);
                continue;
            }

            var value = Convert.ChangeType(record[i], property.PropertyType, CultureInfo.CurrentCulture);
            property.SetValue(target, value);
        }

        return validationResults;
    }

    private static IEnumerable<ValidationModel> GenerateModel<T>(string[] record, T target, int rowNumber)
        where T : class
    {
        var validationHelper = new ValidationHelper();
        var validationResults = new List<ValidationModel>();

        var properties = typeof(T).GetProperties();

        for (var i = 0; i < properties.Length; i++)
        {
            int index = i;
            var property = properties[index];

            var customAttributes = property.GetCustomAttributesData();

            if (customAttributes.Count != 0)
            {
                var csvColumnAttribute = customAttributes.SingleOrDefault(a => a.AttributeType == typeof(HeaderAttribute));
                var indexValue = csvColumnAttribute?.ConstructorArguments
                    .FirstOrDefault(x => x.ArgumentType == typeof(int))
                    .Value;

                if (indexValue is not null)
                {
                    index = (int)indexValue - 1;
                }
            }

            var validationResult = validationHelper.Validate(record[index], property, rowNumber);

            if (validationResult != null)
            {
                validationResults.Add(validationResult);
                continue;
            }

            if (property.PropertyType == typeof(DateOnly))
            {
                var date = DateOnly.Parse(record[index], CultureInfo.CurrentCulture);
                property.SetValue(target, date);
                continue;
            }

            var value = Convert.ChangeType(record[index], property.PropertyType, CultureInfo.InvariantCulture);
            property.SetValue(target, value);
        }

        return validationResults;
    }

    private static PropertyInfo? GetProperty(List<string> header, PropertyInfo[] properties, int index)
    {
        var property = properties.FirstOrDefault(p =>
            p.GetCustomAttributes(typeof(HeaderAttribute), false).FirstOrDefault() is HeaderAttribute headerAttribute &&
            !string.IsNullOrEmpty(headerAttribute.Name) &&
            headerAttribute.Name.Equals(header[index], StringComparison.OrdinalIgnoreCase));

        if (property == null)
        {
            property = properties.FirstOrDefault(p => p.Name.Equals(header[index], StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return property;
            }
        }

        return property;
    }

    private static List<string> GetContent(string filePath)
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
