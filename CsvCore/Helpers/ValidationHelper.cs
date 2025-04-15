using System.Reflection;
using CsvCore.Models;

namespace CsvCore.Helpers;

public class ValidationHelper
{
    public ValidationModel? Validate(string? value, PropertyInfo property, int rowNumber)
    {
        // if a property is nullable and the value is empty, skip the validation
        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
            string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // if a property is not nullable and the value is empty, add a validation error
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationModel
            {
                RowNumber = rowNumber,
                PropertyName = property.Name,
                ConversionError = $"The value for {property.Name} cannot be null or empty."
            };
        }

        // if a property is not nullable and the value is not empty, try to parse the value
        return TryParse(value, property, rowNumber);
    }

    private ValidationModel? TryParse(string value, PropertyInfo property, int rowNumber)
    {
        if (property.PropertyType == typeof(bool))
        {
            if(value == "1")
            {
                return null;
            }

            if(value == "0")
            {
                return null;
            }

            if (!bool.TryParse(value, out _))
            {
                return GenerateValidationModel(value, property, rowNumber);
            }
        }

        if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
        {
            if (!int.TryParse(value, out _))
            {
                return GenerateValidationModel(value, property, rowNumber);
            }
        }

        if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?) ||
            property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?) ||
            property.PropertyType == typeof(float) || property.PropertyType == typeof(float?))
        {
            if (!double.TryParse(value, out _))
            {
                return GenerateValidationModel(value, property, rowNumber);
            }
        }

        if (property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?))
        {
            if (!DateOnly.TryParse(value, out _))
            {
                return GenerateValidationModel(value, property, rowNumber);
            }
        }

        return null;
    }

    private static ValidationModel GenerateValidationModel(string value, PropertyInfo property, int rowNumber) =>
        new()
        {
            RowNumber = rowNumber,
            PropertyName = property.Name,
            ConversionError = $"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}."
        };
}
