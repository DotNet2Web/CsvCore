using CsvCore.Attributes;

namespace CsvCore.Specs.Models;

public class AddressModel
{
    [Header(3)]
    public string Street { get; set; }

    [Header(4)]
    public int HouseNumber { get; set; }

    [Header(5)]
    public string HouseNumberAddition { get; set; }

    [Header(6)]
    public string Zipcode { get; set; }

    [Header(7)]
    public string City { get; set; }
}
