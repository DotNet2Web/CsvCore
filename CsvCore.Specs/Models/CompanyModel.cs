using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class CompanyModel
{
    [Header(1)]
    public string Name { get; set; }

    [Header(1)]
    public string ChamberOfCommerceNumber { get; set; }

    public AddressModel Adddress { get; set; }

    public ContactModel Contact { get; set; }
}
