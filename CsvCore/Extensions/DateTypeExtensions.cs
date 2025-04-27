using System.Globalization;
using System.Reflection;

namespace CsvCore.Extensions;

public static class DateTypeExtensions
{
    public static bool ConvertToDateTypes<T>(this string dateTime, string dateFormat, PropertyInfo property, T target) where T : class
    {
        if (property.PropertyType == typeof(DateOnly))
        {
            var date = !string.IsNullOrEmpty(dateFormat)
                ? DateOnly.ParseExact(dateTime, dateFormat, CultureInfo.CurrentCulture)
                : DateOnly.Parse(dateTime, CultureInfo.CurrentCulture);

            property.SetValue(target, date);

            return true;
        }

        if (property.PropertyType != typeof(DateTime))
        {
            return false;
        }

        var result = !string.IsNullOrEmpty(dateFormat)
            ? DateTime.ParseExact(dateTime, dateFormat, CultureInfo.CurrentCulture)
            : DateTime.Parse(dateTime, CultureInfo.CurrentCulture);

        property.SetValue(target, result);

        return true;
    }
}
