using System;
using System.ComponentModel.DataAnnotations;

namespace CsvCore.Specs.Db.Entities;

public class Employee
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string Surname { get; set; }

    public DateOnly BirthDate { get; set; }

    public string Email { get; set; }
}
