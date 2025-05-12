using System;
using CsvCore.Attributes;
using CsvCore.Specs.Models.Enums;

namespace CsvCore.Specs.Models;

public class CarResultModel
{
    [Header(1)]
    public Guid Id { get; set; }

    [Header(2)]
    public string Manufacturer { get; set; }

    [Header(3)]
    public string Model { get; set; }

    [Header(4)]
    public string Vin { get; set; }

    [Header(5)]
    public int YearOfConstruction { get; set; }

    [Header(6)]
    public int Mileage { get; set; }

    [Header(7)]
    public Fuel Fuel { get; set; }
}
