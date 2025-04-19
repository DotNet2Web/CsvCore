using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class NotMatchingPersonModel
{
    [Position(1)]
    public string Name { get; set; }

    [Position(0)]
    public string Surname { get; set; }

    [Position(2)]
    public DateOnly BirthDate { get; set; }

    [Position(3)]
    public string Email { get; set; }
}
