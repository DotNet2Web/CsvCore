using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class CsvCustomHeaderModel
{
    [Header("Firstname")]
    public string Name { get; set; }

    [Header("family_name")]
    public string Surname { get; set; }

    [Header("dateOfBirth")]
    public DateOnly BirthDate { get; set; }

    [Header("email contact")]
    public string Email { get; set; }
}
