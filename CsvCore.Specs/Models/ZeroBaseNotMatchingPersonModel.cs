using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class ZeroBaseNotMatchingPersonModel
{
    [Header(1)]
    public string Name { get; set; }

    [Header(0)]
    public string Surname { get; set; }

    [Header(2)]
    public DateOnly BirthDate { get; set; }

    [Header(3)]
    public string Email { get; set; }
}
