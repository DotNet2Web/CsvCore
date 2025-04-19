using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class NotMatchingPersonModel
{
    [Header(2)]
    public string Name { get; set; }

    [Header(1)]
    public string Surname { get; set; }

    [Header(3)]
    public DateOnly BirthDate { get; set; }

    [Header(4)]
    public string Email { get; set; }
}
