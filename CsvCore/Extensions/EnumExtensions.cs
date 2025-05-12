using System.Reflection;

namespace CsvCore.Extensions;

public static class EnumExtensions
{
    public static bool ConvertToEnum<T>(this string enumValue, PropertyInfo property, T target)
        where T : class
    {
        if (!property.PropertyType.IsEnum)
        {
            return false;
        }

        var enumType = property.PropertyType;

        if (int.TryParse(enumValue, out var intValue))
        {
            var enumName = Enum.GetName(enumType, intValue);

            if(!Enum.TryParse(enumType, enumName, out _))
            {
                return false;
            }

            if (enumName != null)
            {
                var enumValueParsed = Enum.Parse(enumType, enumName);
                property.SetValue(target, enumValueParsed);
                return true;
            }
        }
        else
        {
            if(!Enum.TryParse(enumType, enumValue, out _))
            {
                return false;
            }

            var enumValueParsed = Enum.Parse(enumType, enumValue);


            property.SetValue(target, enumValueParsed);
            return true;
        }

        return false;
    }
}
