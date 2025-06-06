using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Specs.Models.CsvContent;

[ExcludeFromCodeCoverage]
public class CsvContentDateTimeModel
{
    public string Name { get; set; }

    public string CreatedOn { get; set; }

    public string ModifiedOn { get; set; }
}
