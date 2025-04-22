using System;
using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class PersonCustomNames
{
    [Header(0, "First_Name")]
    public string Name { get; set; }

    [Header(1, "Family_Name")]
    public string Surname { get; set; }

    [Header(2, "Date_Of_Birth")]
    public DateOnly BirthDate { get; set; }

    [Header(3, "Contact_Email")]
    public string Email { get; set; }
}
