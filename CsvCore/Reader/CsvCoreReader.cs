using System.Globalization;
using System.Reflection;
using CsvCore.Attributes;
using CsvCore.Exceptions;
using CsvCore.Extensions;
using CsvCore.Helpers;
using CsvCore.Models;
using CsvCore.Writer;

namespace CsvCore.Reader;

public class CsvCoreReader : ICsvCoreReader
{
    private string? delimiter;
    private bool hasHeaderRecord = true;
    private string errorFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
    private bool validate;
    private string? dateTimeFormat;

    /// <summary>
    /// Use this method to set the delimiter for the CSV file.
    /// </summary>
    /// <param name="customDelimiter"></param>
    /// <returns></returns>
    public CsvCoreReader UseDelimiter(char customDelimiter)
    {
        delimiter = customDelimiter.ToString();
        return this;
    }

    /// <summary>
    /// Use this method to tell us that the CSV file does not have a header record.
    /// </summary>
    /// <returns></returns>
    public CsvCoreReader WithoutHeader()
    {
        hasHeaderRecord = false;
        return this;
    }

    /// <summary>
    /// Use this method to set the date format for DateTime and DateOnly properties.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public CsvCoreReader SetDateTimeFormat(string format)
    {
        dateTimeFormat = format;
        return this;
    }

    /// <summary>
    /// Use this method to tell us that the CSV file does not have a header record.
    /// </summary>
    /// <returns></returns>
    public CsvCoreReader Validate(string? errorPath = null)
    {
        errorFolderPath = string.IsNullOrEmpty(errorPath) ? Path.Combine(Directory.GetCurrentDirectory(), "Errors") : errorPath;

        if (!Directory.Exists(errorFolderPath))
        {
            Directory.CreateDirectory(errorFolderPath);
        }

        validate = true;
        return this;
    }

    /// <summary>
    /// Read the csv file and map it to the model.
    /// </summary>
    /// <param name="filePath">The full path to the csv file</param>
    /// <typeparam name="T">The result model</typeparam>
    /// <returns></returns>
    /// <exception cref="MissingFileException"></exception>
    public async Task<IEnumerable<T>> ReadAsync<T>(string filePath) where T : class
    {
        var result = new List<T>();

        await foreach (var target in ReadStreamAsync<T>(filePath))
        {
            result.Add(target);
        }

        return result;
    }

    /// <summary>
    /// Streams mapped records without retaining the complete result set in memory.
    /// </summary>
    private async IAsyncEnumerable<T> ReadStreamAsync<T>(string filePath) where T : class
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        var properties = typeof(T).GetProperties();
        PropertyInfo?[]? headerProperties = null;

        (int startPosition, List<PropertyInfo> orderedProperties) = OrderProperties<T>();

        var rowNumber = 1;
        var validationResults = validate ? new List<ValidationModel>() : null;

        await foreach (var line in GetContentAsync(filePath))
        {
            if (hasHeaderRecord && headerProperties is null)
            {
                headerProperties = GetHeaderProperties(line.Split(delimiter), properties);
                continue;
            }

            var record = line.Split(delimiter);
            var target = Activator.CreateInstance<T>();
            var hasValidationErrors = hasHeaderRecord
                ? GenerateModelBasedOnHeader(headerProperties!, record, target, rowNumber, validationResults)
                : GenerateModel(record, target, rowNumber, startPosition, orderedProperties, validationResults);

            if (!hasValidationErrors)
            {
                yield return target;
            }

            rowNumber++;
        }

        if (validationResults is not { Count: > 0 })
        {
            yield break;
        }

        var errorFile = Path.GetFileNameWithoutExtension(filePath);

        await new CsvCoreWriter()
            .UseDelimiter(char.Parse(delimiter))
            .WriteAsync(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"), validationResults);
    }

    /// <summary>
    /// Read the csv file and map it to the model.
    /// </summary>
    /// <param name="filePath">The full path to the csv file</param>
    /// <typeparam name="T">The result model</typeparam>
    /// <returns></returns>
    /// <exception cref="MissingFileException"></exception>
    public IEnumerable<T> Read<T>(string filePath) where T : class
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        return ReadStreamCore<T>(filePath);
    }

    private IEnumerable<T> ReadStreamCore<T>(string filePath) where T : class
    {
        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        var properties = typeof(T).GetProperties();
        var headerProperties = Array.Empty<PropertyInfo?>();
        var headerRead = !hasHeaderRecord;
        (int startPosition, List<PropertyInfo> orderedProperties) = OrderProperties<T>();

        var rowNumber = 1;

        var validationResults = validate ? new List<ValidationModel>() : null;

        foreach (var line in GetContent(filePath))
        {
            if (hasHeaderRecord && !headerRead)
            {
                var headerItems = line.Split(delimiter);
                headerProperties = GetHeaderProperties(headerItems, properties);
                headerRead = true;
                continue;
            }

            var record = line.Split(delimiter);
            var target = Activator.CreateInstance<T>();

            var hasValidationErrors = hasHeaderRecord
                ? GenerateModelBasedOnHeader(headerProperties, record, target, rowNumber, validationResults)
                : GenerateModel(record, target, rowNumber, startPosition, orderedProperties, validationResults);

            if (hasValidationErrors)
            {
                rowNumber++;

                continue;
            }

            yield return target;

            rowNumber++;
        }

        if (validationResults is null || validationResults.Count == 0)
        {
            yield break;
        }

        var errorFile = Path.GetFileNameWithoutExtension(filePath);

        new CsvCoreWriter()
            .UseDelimiter(char.Parse(delimiter))
            .Write(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"), validationResults);

    }

    /// <summary>
    /// Use this method to validate the csv file without mapping it to the model.
    /// </summary>
    /// <param name="filePath">The full path to the csv file</param>
    /// <typeparam name="T">The result model, just for checking if it is possible to map</typeparam>
    /// <returns>A list of records that are invalid</returns>
    /// <exception cref="MissingFileException"></exception>
    public async Task<IEnumerable<ValidationModel>> IsValidAsync<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var validationResults = new List<ValidationModel>();

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        var rowNumber = 1;

        var validationHelper = new ValidationHelper();
        var properties = typeof(T).GetProperties();
        PropertyInfo?[]? headerProperties = null;

        await foreach (var line in GetContentAsync(filePath))
        {
            if (hasHeaderRecord && headerProperties is null)
            {
                headerProperties = GetHeaderProperties(line.Split(delimiter), properties);
                continue;
            }

            if (!hasHeaderRecord)
            {
                continue;
            }

            var record = line.Split(delimiter);

            for (var i = 0; i < headerProperties!.Length; i++)
            {
                var property = headerProperties[i];

                if (property == null)
                {
                    continue;
                }

                var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults.Add(validationResult);
                }
            }

            rowNumber++;
        }

        return validationResults;
    }

    /// <summary>
    /// Use this method to validate the csv file without mapping it to the model.
    /// </summary>
    /// <param name="filePath">The full path to the csv file</param>
    /// <typeparam name="T">The result model, just for checking if it is possible to map</typeparam>
    /// <returns>A list of records that are invalid</returns>
    /// <exception cref="MissingFileException"></exception>
    public IEnumerable<ValidationModel> IsValid<T>(string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var validationResults = new List<ValidationModel>();

        delimiter ??= CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        var rowNumber = 1;

        var validationHelper = new ValidationHelper();
        var properties = typeof(T).GetProperties();
        PropertyInfo?[]? headerProperties = null;

        foreach (var line in GetContent(filePath))
        {
            if (hasHeaderRecord && headerProperties is null)
            {
                headerProperties = GetHeaderProperties(line.Split(delimiter), properties);
                continue;
            }

            if (!hasHeaderRecord)
            {
                continue;
            }

            var record = line.Split(delimiter);

            for (var i = 0; i < headerProperties!.Length; i++)
            {
                var property = headerProperties[i];

                if (property == null)
                {
                    continue;
                }

                var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults.Add(validationResult);
                }
            }

            rowNumber++;
        }

        return validationResults;
    }

    private bool GenerateModelBasedOnHeader<T>(PropertyInfo?[] headerProperties, string[] record, T target,
        int rowNumber, List<ValidationModel>? validationResults)
        where T : class
    {
        var validationHelper = new ValidationHelper();
        var hasValidationErrors = false;

        for (var i = 0; i < headerProperties.Length; i++)
        {
            var property = headerProperties[i];

            if (property == null)
            {
                continue;
            }

            if (validate)
            {
                var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults?.Add(validationResult);
                    hasValidationErrors = true;
                    continue;
                }
            }

            if (record[i].ConvertToDateTypes(dateTimeFormat, property, target))
            {
                continue;
            }

            if (record[i].ConvertToGuid(property, target))
            {
                continue;
            }

            if (record[i].ConvertToEnum(property, target))
            {
                continue;
            }

            var value = Convert.ChangeType(record[i], property.PropertyType, CultureInfo.CurrentCulture);
            property.SetValue(target, value);
        }

        return hasValidationErrors;
    }

    private bool GenerateModel<T>(string[] record, T target, int rowNumber, int startPosition,
        List<PropertyInfo> properties, List<ValidationModel>? validationResults)
        where T : class
    {
        var validationHelper = new ValidationHelper();
        var hasValidationErrors = false;

        for (var i = 0; i < properties.Count; i++)
        {
            var property = properties[i];

            var index = DetermineIndex(property, startPosition, i);

            if (validate)
            {
                var validationResult = validationHelper.Validate(record[index], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults?.Add(validationResult);
                    hasValidationErrors = true;
                    continue;
                }
            }

            if (record[index].ConvertToDateTypes(dateTimeFormat, property, target))
            {
                continue;
            }

            if (record[i].ConvertToGuid(property, target))
            {
                continue;
            }

            var value = Convert.ChangeType(record[index], property.PropertyType, CultureInfo.InvariantCulture);
            property.SetValue(target, value);
        }

        return hasValidationErrors;
    }

    private static int DetermineIndex(PropertyInfo property, int startPosition, int index)
    {
        var customAttributes = property.GetCustomAttributesData();

        if (customAttributes.Count == 0)
        {
            return index;
        }

        var csvColumnAttribute = customAttributes.SingleOrDefault(a => a.AttributeType == typeof(HeaderAttribute));

        var indexValue = csvColumnAttribute?.ConstructorArguments
            .FirstOrDefault(x => x.ArgumentType == typeof(int))
            .Value;

        if (indexValue is null)
        {
            return index;
        }

        if (startPosition > 0)
        {
            index = (int)indexValue - 1;
        }
        else
        {
            index = (int)indexValue;
        }

        return index;
    }

    private static (int startPosition, List<PropertyInfo> sortedProperties) OrderProperties<T>()
    {
        int? startPosition = 0;
        var properties = typeof(T).GetProperties().ToList();

        var hasHeaderAttribute = properties
            .Any(p => p.GetCustomAttributes(typeof(HeaderAttribute), false).FirstOrDefault() is HeaderAttribute);

        if (hasHeaderAttribute)
        {
            properties = properties
                .Where(p => p.GetCustomAttributes(typeof(HeaderAttribute), false).FirstOrDefault() is HeaderAttribute)
                .OrderBy(p => ((HeaderAttribute)p.GetCustomAttributes(typeof(HeaderAttribute), false).First()).Position)
                .ToList();

            startPosition = properties.First().GetCustomAttribute<HeaderAttribute>()?.Position;
        }

        return (startPosition!.Value, properties);
    }

    private static PropertyInfo?[] GetHeaderProperties(string[] header, PropertyInfo[] properties)
    {
        var result = new PropertyInfo?[header.Length];

        for (var i = 0; i < header.Length; i++)
        {
            result[i] = properties.FirstOrDefault(p =>
                p.GetCustomAttributes(typeof(HeaderAttribute), false).FirstOrDefault() is HeaderAttribute headerAttribute &&
                !string.IsNullOrEmpty(headerAttribute.Name) &&
                headerAttribute.Name.Equals(header[i], StringComparison.OrdinalIgnoreCase))
                ?? properties.FirstOrDefault(p => p.Name.Equals(header[i], StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }

    private IEnumerable<string> GetContent(string filePath)
    {
        using var reader = new StreamReader(filePath, new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.ReadWrite
        });

        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    private static async IAsyncEnumerable<string> GetContentAsync(string filePath)
    {
        using var reader = new StreamReader(filePath, new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.ReadWrite
        });

        while (await reader.ReadLineAsync() is { } line)
        {
            yield return line;
        }
    }
}
