using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(int position = 0, string? name = null) : Attribute
{
    public int? Position { get; } = position;

    public string? Name { get; } = name;
}
