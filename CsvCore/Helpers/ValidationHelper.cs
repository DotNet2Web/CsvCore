using System.Globalization;
using System.Reflection;
using CsvCore.Models;

namespace CsvCore.Helpers;

public class ValidationHelper
{
    private const string Canbenull = "CanBeNullAttribute";
    private const string Allownull = "AllowNullAttribute";
    private const string Maybenull = "MaybeNullAttribute";

    public ValidationModel? Validate(string? value, PropertyInfo property, int rowNumber, string? dateFormat)
    {
        var isNullable = IsNullable(property);

        // if a property is nullable and the value is empty, skip the validation
        if (isNullable && string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // if a property is not nullable and the value is empty, add a validation error
        if (!isNullable && string.IsNullOrWhiteSpace(value))
        {
            if (property.PropertyType == typeof(string))
            {
                return GenerateValidationModel(value ?? string.Empty, property, rowNumber);
            }

            return new ValidationModel
            {
                RowNumber = rowNumber,
                PropertyName = property.Name,
                ConversionError = $"The value for {property.Name} cannot be null or empty."
            };
        }

        // if a property is not nullable and the value is not empty, try to parse the value
        return TryParse(value!, property, rowNumber, dateFormat);
    }

    private static bool IsNullable(PropertyInfo property)
    {
        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return true;
        }

        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
        {
            return true;
        }

        if (property.CustomAttributes.Any(a =>
                a.AttributeType.Name is Canbenull or Allownull or Maybenull))
        {
            return true;
        }

        var nullability = new NullabilityInfoContext().Create(property);
        if (nullability.WriteState == NullabilityState.Nullable || nullability.ReadState == NullabilityState.Nullable)
        {
            return true;
        }

        return false;
    }

    private ValidationModel? TryParse(string value, PropertyInfo property, int rowNumber, string? dateFormat)
    {
        if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        {
            switch (value)
            {
                case "1":
                case "0":
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

        if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
        {
            if (!decimal.TryParse(value, out _))
            {
                return GenerateValidationModel(value, property, rowNumber);
            }
        }

        if (property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?))
        {
            if(!string.IsNullOrEmpty(dateFormat))
            {
                if (!DateOnly.TryParseExact(value, dateFormat,  CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return GenerateValidationModel(value, property, rowNumber);
                }
            }
            else
            {
                // If no date format is provided, use the default parsing
                if (!DateOnly.TryParse(value, out _))
                {
                    return GenerateValidationModel(value, property, rowNumber);
                }
            }
        }

        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        {
            if(!string.IsNullOrEmpty(dateFormat))
            {
                if (!DateTime.TryParseExact(value, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return GenerateValidationModel(value, property, rowNumber);
                }
            }
            else
            {
                if (!DateTime.TryParse(value, out _))
                {
                    return GenerateValidationModel(value, property, rowNumber);
                }
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
