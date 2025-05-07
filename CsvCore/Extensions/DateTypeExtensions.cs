using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CsvCore.Extensions;

public static class DateTypeExtensions
{
    private const string DateTimePattern = @"\b(?:\d{4}[-/]\d{2}[-/]\d{2}|(?:\d{1,2}[-/.]){2}\d{4}|\d{8})(?:[ T]\d{2}:\d{2}(?::\d{2})?)\b";
    private const string DateOnlyPattern = @"\b(?:\d{4}[-/]\d{2}[-/]\d{2}|(?:\d{1,2}[-/.]){2}\d{4}|\d{8})\b";

    public static bool ConvertToDateTypes<T>(this string dateTime, string? dateFormat, PropertyInfo property, T target)
        where T : class
    {
        return SetDateOnly(dateTime, dateFormat, property, target) ||
               SetDateTime(dateTime, dateFormat, property, target);
    }

    private static bool SetDateOnly<T>(string dateTime, string? dateFormat, PropertyInfo property, T target) where T : class
    {
        if (property.PropertyType != typeof(DateOnly))
        {
            return false;
        }

        if (string.IsNullOrEmpty(dateFormat))
        {
            if (Regex.IsMatch(dateTime, DateOnlyPattern))
            {
                property.SetValue(target, DateOnly.Parse(dateTime, CultureInfo.CurrentCulture));

                return true;
            }
        }

        if (dateTime.ValidateDateOnly(dateFormat))
        {
            property.SetValue(target, DateOnly.ParseExact(dateTime, dateFormat!, CultureInfo.CurrentCulture));

            return true;
        }

        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(DateOnly?) &&
            string.IsNullOrWhiteSpace(dateTime))
        {
            property.SetValue(target, null);
            return true;
        }

        property.SetValue(target, DateOnly.MinValue);
        return true;
    }

    private static bool SetDateTime<T>(string dateTime, string? dateFormat, PropertyInfo property, T target) where T : class
    {
        if (property.PropertyType != typeof(DateTime))
        {
            return false;
        }

        if (string.IsNullOrEmpty(dateFormat))
        {
            if (Regex.IsMatch(dateTime, DateTimePattern))
            {
                property.SetValue(target, DateTime.Parse(dateTime, CultureInfo.CurrentCulture));

                return true;
            }
        }

        if (dateTime.ValidateDateTime(dateFormat))
        {
            property.SetValue(target, DateTime.ParseExact(dateTime, dateFormat!, CultureInfo.CurrentCulture));

            return true;
        }

        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(DateTime?) &&
            string.IsNullOrWhiteSpace(dateTime))
        {
            property.SetValue(target, null);
            return true;
        }

        property.SetValue(target, DateTime.MinValue);
        return true;

    }

    private static bool ValidateDateTime(this string dateTime, string? dateFormat)
    {
        return string.IsNullOrEmpty(dateFormat)
            ? DateTime.TryParse(dateTime, out _)
            : DateTime.TryParseExact(dateTime, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    private static bool ValidateDateOnly(this string dateOnly, string? dateFormat)
    {
        return string.IsNullOrEmpty(dateFormat)
            ? DateOnly.TryParse(dateOnly, out _)
            : DateOnly.TryParseExact(dateOnly, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}
