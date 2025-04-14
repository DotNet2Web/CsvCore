using System;

namespace CsvCore.Specs.Models;

public class PersonModel
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public DateOnly BirthDate { get; set; }

    public string Email { get; set; }
}
