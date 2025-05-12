using System;

namespace CsvCore.Specs.Models;

public class DateTypesTestModel
{
    public DateOnly DateOnlyProperty { get; set; }

    public DateTime DateTimeProperty { get; set; }

    public DateOnly? NullableDateOnlyProperty { get; set; }

    public DateTime? NullableDateTimeProperty { get; set; }
}
