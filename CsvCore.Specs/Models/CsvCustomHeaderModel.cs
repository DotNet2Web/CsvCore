using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class CsvCustomHeaderModel
{
    [Header(name: "Firstname")]
    public string Name { get; set; }

    [Header(name: "family_name")]
    public string Surname { get; set; }

    [Header(name: "dateOfBirth")]
    public DateOnly BirthDate { get; set; }

    [Header(name: "email contact")]
    public string Email { get; set; }
}
