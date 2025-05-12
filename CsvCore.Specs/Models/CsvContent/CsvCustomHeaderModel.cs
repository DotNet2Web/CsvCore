using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models.CsvContent;

public class CsvCustomHeaderModel
{
    [Header(0, "Firstname")]
    public string Name { get; set; }

    [Header(1, "family_name")]
    public string Surname { get; set; }

    [Header(2, "dateOfBirth")]
    public DateOnly BirthDate { get; set; }

    [Header(3, "email contact")]
    public string Email { get; set; }
}
