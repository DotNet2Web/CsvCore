using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Attributes;

[ExcludeFromCodeCoverage]
public class CsvPosition : Attribute
{
    public CsvPosition(int index) { }
}
