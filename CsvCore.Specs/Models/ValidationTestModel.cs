using System;

namespace CsvCore.Specs.Models;

public class ValidationTestModel
{
    public int? Id { get; set; }

    public string Name { get; set; }

    public bool Active { get; set; }

    public double Amount { get; set; }

    public DateOnly Date { get; set; }

    public DateTime DateTime { get; set; }
}
