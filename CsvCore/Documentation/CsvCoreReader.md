## Using the CsvCoreReader
If you only need to read a csv file, you can register the `CsvCoreReader` in your IoC container like this:

```csharp
builder.Services.AddScoped<ICsvCoreReader, CsvCoreReader>();
```

Then you can use the `ICsvCoreReader` in your code:

### Example

CSV file
```text
Name;Surname;Birthdate;Email
Foo;Bar;01/01/2025;foo@bar.com
```

```csharp
// A model that matches the data on the same position in the CSV file.
public class ResultModel
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string Birthdate { get; set; }

    public string Email { get; set; }
}
```

```csharp
public class Foo(ICsvCoreReader csvCoreReader)
{
    public void ReadWithHeaderRecordAndCustomDelimiter()
    {
         var results = csvCoreReader
           .UseDelimiter(';') // Specify your custom delimiter.
           .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv")); // Read and map the data to your own model and yes the result is a IEnumerable of your model.
    }

    public void ReadWithoutHeaderRecordAndCustomDelimiter()
    {
         var results = csvCoreReader
           .UseDelimiter(';') // Specify your custom delimiter.
           .WithoutHeader()
           .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv")); // Read and map the data to your own model and yes the result is a IEnumerable of your model.
    }

    public void ReadWithHeaderRecordAndDefaultDelimiter()
    {
         var results = csvCoreReader.Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv")); // Read and map the data to your own model and yes the result is a IEnumerable of your model.
    }
}
```

In case you receive a csv that does not match the model you are reading the data too, then you can use the `Header` attribute in the model.
Using the `Header` attribute you can specify the position of the property in the csv file.

CSV file
```text
Surname;Name;Birthdate;Email
Bar;Foo;01/01/2025;foo@bar.com
```

```csharp
public class NotMatchingPersonModel
{
    [Header(2)] // This property is in the second column in the csv file.
    public string Name { get; set; }

    [Header(1)] // This property is in the first column in the csv file.
    public string Surname { get; set; }

    [Header(3)] // This property is in the third column in the csv file.
    public DateOnly BirthDate { get; set; }

    [Header(4)] // This property is in the fourth column in the csv file.
    public string Email { get; set; }
}
```

When you have the model all setup use the `CsvCoreReader` to read the csv file, see the above section for the example.
