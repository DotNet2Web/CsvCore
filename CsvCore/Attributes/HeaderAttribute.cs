using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}
