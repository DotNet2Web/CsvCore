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
        if (property.PropertyType == typeof(DateOnly))
        {
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
                property.SetValue(target, DateOnly.ParseExact(dateTime, dateFormat!, CultureInfo.InvariantCulture));

                return true;
            }
        }

        if (property.PropertyType != typeof(DateTime))
        {
            return false;
        }

        if (string.IsNullOrEmpty(dateFormat))
        {
            if (Regex.IsMatch(dateTime, DateTimePattern))
            {
                property.SetValue(target, DateTime.Parse(dateTime, CultureInfo.InvariantCulture));

                return true;
            }
        }

        property.SetValue(target, DateTime.ParseExact(dateTime, dateFormat!, CultureInfo.InvariantCulture));

        return true;
    }

    public static bool ValidateDateTime(this string dateTime, string? dateFormat)
    {
        return string.IsNullOrEmpty(dateFormat)
            ? DateTime.TryParse(dateTime, out _)
            : DateTime.TryParseExact(dateTime, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public static bool ValidateDateOnly(this string dateOnly, string? dateFormat)
    {
        return string.IsNullOrEmpty(dateFormat)
            ? DateOnly.TryParse(dateOnly, out _)
            : DateOnly.TryParseExact(dateOnly, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}
