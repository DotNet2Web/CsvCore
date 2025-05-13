using System;

namespace CsvCore.Specs.Models;

public class CarWithOnlyAFewColumnsSelectedModel
{
    public Guid Id { get; set; }

    public string Manufacturer { get; set; }

    public string Model { get; set; }

    public int Mileage { get; set; }
}
