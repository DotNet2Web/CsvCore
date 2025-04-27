using System;
using System.Diagnostics.CodeAnalysis;

namespace CsvCore.Specs.Models;

[ExcludeFromCodeCoverage]
public class ValidationTestModel
{
    public int? Id { get; set; }

    public string Name { get; set; }

    public bool? Active { get; set; }

    public decimal? Amount { get; set; }

    public DateOnly? Date { get; set; }

    public DateTime? DateTime { get; set; }
}
