using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class NotMatchingPersonModel
{
    [CsvPosition(1)]
    public string Name { get; set; }

    [CsvPosition(0)]
    public string Surname { get; set; }

    [CsvPosition(2)]
    public DateOnly BirthDate { get; set; }

    [CsvPosition(3)]
    public string Email { get; set; }
}
