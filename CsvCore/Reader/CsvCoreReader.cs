using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using CsvCore.Attributes;
using CsvCore.Exceptions;
using CsvCore.Extensions;
using CsvCore.Helpers;
using CsvCore.Models;
using CsvCore.Writer;
using Microsoft.EntityFrameworkCore;

namespace CsvCore.Reader;

public class CsvCoreReader : ICsvCoreReader
{
    private const string DefaultPrimaryKeyName = "id";
    private string? delimiter;
    private bool hasHeaderRecord = true;
    private string errorFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
    private bool validate;
    private string? dateTimeFormat;
    private DbContext? _dbContext;
    private static List<PropertyInfo>? _allComplexTypesProperties = null;

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
    /// Use this method to set the DbContext for the CSV file.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public CsvCoreReader UseDbContext(DbContext dbContext)
    {
        _dbContext = dbContext;
        return this;
    }

    /// <summary>
    /// Read the csv file and map it to the model.
    /// </summary>
    /// <param name="filePath">The fullpath to the csv file</param>
    /// <typeparam name="T">The result model</typeparam>
    /// <returns></returns>
    /// <exception cref="MissingFileException"></exception>
    public async Task<IEnumerable<T>> ReadAsync<T>(string filePath) where T : class
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var lines = await GetContentAsync(filePath);

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
            var recordValidationResults = new List<ValidationModel>();
            var target = Activator.CreateInstance<T>();

            recordValidationResults.AddRange(hasHeaderRecord
                ? GenerateModelBasedOnHeader(headerItems, record, target, rowNumber)
                : GenerateModel(record, target, rowNumber));

            if (recordValidationResults.Any())
            {
                validationResults.AddRange(recordValidationResults);
                recordValidationResults.Clear();

                rowNumber++;

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
            .UseDelimiter(char.Parse(delimiter))
            .Write(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"), validationResults);

        return result;
    }

    /// <summary>
    /// Read the csv file and map it to the model.
    /// </summary>
    /// <param name="filePath">The fullpath to the csv file</param>
    /// <typeparam name="T">The result model</typeparam>
    /// <returns></returns>
    /// <exception cref="MissingFileException"></exception>
    public IEnumerable<T> Read<T>(string filePath) where T : class
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
            var recordValidationResults = new List<ValidationModel>();
            var target = Activator.CreateInstance<T>();

            recordValidationResults.AddRange(hasHeaderRecord
                ? GenerateModelBasedOnHeader(headerItems, record, target, rowNumber)
                : GenerateModel(record, target, rowNumber));

            if (recordValidationResults.Any())
            {
                validationResults.AddRange(recordValidationResults);
                recordValidationResults.Clear();

                rowNumber++;

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
            .UseDelimiter(char.Parse(delimiter))
            .Write(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"), validationResults);

        return result;
    }

    public async Task PersistAsync<TEntity>(string filePath) where TEntity : class
    {
        if (_dbContext is null)
        {
            throw new DbContextNotSetException("DbContext is not set. Use 'UseDbContext' method to set the DbContext.");
        }

        var entitiesToAdd = await ReadAsync<TEntity>(filePath);
        var dbSet = _dbContext.Set<TEntity>();

        var existingEntities = await dbSet.ToListAsync();

        if (!existingEntities.Any())
        {
            _dbContext.AddRange(entitiesToAdd.ToList());
        }
        else
        {
            AddNewRecordsOnly(entitiesToAdd.ToList(), existingEntities);
        }

        await _dbContext.SaveChangesAsync();
    }

    public void Persist<TEntity>(string filePath) where TEntity : class
    {
        if (_dbContext is null)
        {
            throw new DbContextNotSetException("DbContext is not set. Use 'UseDbContext' method to set the DbContext.");
        }

        var entitiesToAdd = Read<TEntity>(filePath);
        var dbSet = _dbContext.Set<TEntity>();

        var existingEntities = dbSet.ToList();

        if (!existingEntities.Any())
        {
            _dbContext.AddRange(entitiesToAdd);
        }
        else
        {
            AddNewRecordsOnly(entitiesToAdd, existingEntities);
        }

        _dbContext.SaveChanges();
    }

    private void AddNewRecordsOnly<TEntity>(IEnumerable<TEntity> entitiesToAdd, List<TEntity> existingEntities)
        where TEntity : class
    {
        foreach (var entity in entitiesToAdd)
        {
            var entityExists = existingEntities.Any(ee =>
                ee.GetType().GetProperties()
                    .Where(p => !p.GetCustomAttributes(typeof(KeyAttribute), false).Any() &&
                                p.Name.ToLower() != DefaultPrimaryKeyName)
                    .All(p => p.GetValue(ee)?.ToString() == p.GetValue(entity)?.ToString()));

            if (!entityExists)
            {
                _dbContext!.Add(entity);
            }
        }
    }

    /// <summary>
    /// Use this method to validate the csv file without mapping it to the model.
    /// </summary>
    /// <param name="filePath">The fullpath to the csv file</param>
    /// <typeparam name="T">The result model, just for checking if it is possible to map</typeparam>
    /// <returns>A list of records that are invalid</returns>
    /// <exception cref="MissingFileException"></exception>
    public async Task<IEnumerable<ValidationModel>> IsValidAsync<T>(string filePath)
        where T : class
    {
        if (!File.Exists(filePath))
        {
            throw new MissingFileException($"The file '{filePath}' does not exist.");
        }

        var validationResults = new List<ValidationModel>();

        var lines = await GetContentAsync(filePath);

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

                    var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

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

    /// <summary>
    /// Use this method to validate the csv file without mapping it to the model.
    /// </summary>
    /// <param name="filePath">The fullpath to the csv file</param>
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

                    var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

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

    private IEnumerable<ValidationModel> GenerateModelBasedOnHeader<T>(List<string> header, string[] record, T target,
        int rowNumber)
        where T : class
    {
        var propertiesOfTheResultModel = typeof(T).GetProperties();
        var validationHelper = new ValidationHelper();
        var validationResults = new List<ValidationModel>();

        object? childTarget = null;

        for (var i = 0; i < header.Count; i++)
        {
            var property = GetProperty(header, propertiesOfTheResultModel, i);

            if (property == null)
            {
                continue;
            }

            if (validate)
            {
                var validationResult = validationHelper.Validate(record[i], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults.Add(validationResult);
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

            if (property.DeclaringType != typeof(T))
            {
                if(childTarget != null && property.DeclaringType != childTarget.GetType())
                {
                    childTarget = null;
                }

                childTarget ??= Activator.CreateInstance(property.DeclaringType!);

                var childProperty = childTarget!.GetType().GetProperty(property.Name);

                if (childProperty != null)
                {
                    childProperty.SetValue(childTarget, value);

                    var complexTypeProperty = propertiesOfTheResultModel
                        .First(p => p.PropertyType.IsClass &&
                                    p.PropertyType != typeof(string) &&
                                    p.PropertyType == property.DeclaringType);

                    complexTypeProperty.SetValue(target, childTarget);

                    continue;
                }
            }

            property.SetValue(target, value);
        }

        return validationResults;
    }

    private IEnumerable<ValidationModel> GenerateModel<T>(string[] record, T target, int rowNumber)
        where T : class
    {
        var validationHelper = new ValidationHelper();
        var validationResults = new List<ValidationModel>();
        object? childTarget = null;

        (int startPosition, List<PropertyInfo> propertiesOfTheResultModel) = OrderProperties<T>();

        for (var i = 0; i < propertiesOfTheResultModel.Count; i++)
        {
            var property = propertiesOfTheResultModel[i];

            var index = DetermineIndex(property, startPosition, i);

            if (validate)
            {
                var validationResult = validationHelper.Validate(record[index], property, rowNumber, dateTimeFormat);

                if (validationResult != null)
                {
                    validationResults.Add(validationResult);
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

            var value = Convert.ChangeType(record[index], property.PropertyType, CultureInfo.InvariantCulture);

            if (property.DeclaringType != typeof(T))
            {
                childTarget ??= Activator.CreateInstance(property.DeclaringType!);

                var childProperty = childTarget!.GetType().GetProperty(property.Name);

                if (childProperty != null)
                {
                    childProperty.SetValue(childTarget, value);

                    var complexTypeProperty = propertiesOfTheResultModel
                        .First(p => p.PropertyType.IsClass &&
                                    p.PropertyType != typeof(string) &&
                                    p.PropertyType == property.DeclaringType);

                    complexTypeProperty.SetValue(target, childTarget);

                    continue;
                }
            }

            property.SetValue(target, value);
        }

        return validationResults;
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

    private static PropertyInfo? GetProperty(List<string> header, PropertyInfo[] propertiesOfTheResultModel, int index)
    {
        var property = propertiesOfTheResultModel.FirstOrDefault(p =>
            p.GetCustomAttributes(typeof(HeaderAttribute), false).FirstOrDefault() is HeaderAttribute headerAttribute &&
            !string.IsNullOrEmpty(headerAttribute.Name) &&
            headerAttribute.Name.Equals(header[index], StringComparison.OrdinalIgnoreCase));

        if (property != null)
        {
            return property;
        }

        property = propertiesOfTheResultModel.FirstOrDefault(p =>
            p.Name.Equals(header[index], StringComparison.OrdinalIgnoreCase));

        if (property != null)
        {
            return property;
        }

        var hasComplexTypes = propertiesOfTheResultModel.Any(p => p.PropertyType.IsClass &&
                                                                  p.PropertyType != typeof(string));

        if (!hasComplexTypes)
        {
            return property;
        }

        var complexTypeProperties = propertiesOfTheResultModel
            .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string))
            .ToList();

        if (_allComplexTypesProperties is null)
        {
            _allComplexTypesProperties = [];

            foreach (PropertyInfo complexTypeProperty in complexTypeProperties)
            {
                _allComplexTypesProperties.AddRange(complexTypeProperty.PropertyType.GetProperties());
            }
        }

        property = GetProperty(header, _allComplexTypesProperties.ToArray(), index);

        return property;
    }

    private List<string> GetContent(string filePath)
    {
        var lines = new List<string>();

        using var reader = new StreamReader(filePath,
            new FileStreamOptions { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.ReadWrite });

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }

    private static async Task<List<string>> GetContentAsync(string filePath)
    {
        var lines = new List<string>();

        using var reader = new StreamReader(filePath,
            new FileStreamOptions { Access = FileAccess.Read, Mode = FileMode.Open, Share = FileShare.ReadWrite });

        while (await reader.ReadLineAsync() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }
}
