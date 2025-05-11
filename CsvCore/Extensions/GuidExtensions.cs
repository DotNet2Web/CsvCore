using System.Reflection;
using System.Text.RegularExpressions;

namespace CsvCore.Extensions;

public static class GuidExtensions
{
    private const string GuidPattern = @"\b[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}\b";

    public static bool ConvertToGuid<T>(this string guid, PropertyInfo property, T target)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(guid) || (property.PropertyType.UnderlyingSystemType != typeof(Guid?) &&
                                                property.PropertyType.UnderlyingSystemType != typeof(Guid)))
        {
            return false;
        }

        if (Regex.IsMatch(guid, GuidPattern))
        {
            property.SetValue(target, Guid.Parse(guid));
            return true;
        }

        return false;
    }
}
