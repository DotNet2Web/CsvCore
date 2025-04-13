namespace CsvCore.Models;

public class ValidationModel
{
    public int RowNumber { get; set; }

    public string? PropertyName { get; set; }

    public string? ConversionError { get; set; }
}
