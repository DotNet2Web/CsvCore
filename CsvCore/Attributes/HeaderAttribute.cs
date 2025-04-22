using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(int position, string? name = null) : Attribute
{
    public int Position { get; } = position;

    public string? Name { get; } = name;
}
