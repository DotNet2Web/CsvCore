using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class ContactModel
{
    [Header(8)]
    public string Email { get; set; }

    [Header(9)]
    public string Phonenumber { get; set; }
}
