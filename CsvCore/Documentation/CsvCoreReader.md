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

### Validations

Ofcourse you will receive csv files that are not valid, so we added some validations to the reader.

We will validate the data before adding them to the result models, any record that cant be parsed correctly will be added to errors.csv file.

The file will be stored at the location your application will be run. The filename will be the same as the original file, but we just add `_errors` to it.

If you need those error files to be written somewhere else simply use the `.SetErrorPath("AnyPath")` method on the reader

### Example

```csharp
    var result = csvCoreReader
        .SetErrorPath(@"C:\Temp\Errors") // your errors will be stored in here, ofcourse you would put this in a configuration file ;)
        .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv"));

    or

    var result = csvCoreReader
        .SetErrorPath() // your errors will be stored in the same location as the application is run.
        .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv"));

    or

    var result = csvCoreReader
        .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv")); // your errors will be stored in the same location as the application is run.
```

If you want to validate the csv file before reading it, you can use the `IsValid` method

### Example

```csharp
   var result = csvCoreReader
       .IsValid<ResultModel>(Path.Combine("AnyPath", "YourFile.csv"));
```

The `IsValid` method will return a `List<ValidationModel>` containing:
- The line number of the invalid record.
- The property name of the invalid record.
- The reason why the data could not be parsed stored in the error message property

This could be handy in numerous ways.

And how about skipping the validation. If you want to skip the validation, you can use the `SkipValidation` method.
### Example

```csharp
   var result = csvCoreReader
       .SkipValidation()
       .Read<ResultModel>(Path.Combine("AnyPath", "YourFile.csv"));
```

This will skip the validation and read the csv file without validating the data.
The result will be a list of `ResultModel` objects, but the data will not be validated.

**A little note about the validation:**
- _If you have a non-nullable dateonly / datetime property in your model, and the csv file contains a null value, the reader will set these properties to their MinValues._

This way you can do whatever you want with the result.
