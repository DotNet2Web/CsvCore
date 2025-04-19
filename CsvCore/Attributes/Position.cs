using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Attributes;

[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
public class Position(int index) : Attribute
{
    public int Index { get; } = index;
}
