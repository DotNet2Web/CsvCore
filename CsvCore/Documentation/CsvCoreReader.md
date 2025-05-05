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

If you don't want to validate the csv file before reading it, just read the file like described above.

We will validate the data before adding them to the result models, any record that cant be parsed correctly will be added to errors.csv file.

The file will be stored at the location your application will be run. The filename will be the same as the original file, but we just add `_errors` to it.

If you need those error files to be written somewhere else simply use the `.WriteErrorsAt("AnyPath")` method on the reader

### Example

```csharp
    var result = csvCoreReader
        .SetErrorPath(@"C:\Temp\Errors") // your errors will be stored in here, ofcourse you would put this in a configuration file ;)
        .Read<PersonModel>(filePath);

    or

    var result = csvCoreReader
        .SetErrorPath() // your errors will be stored in the same location as the application is run.
        .Read<PersonModel>(filePath);

    or

    var result = csvCoreReader
        .Read<PersonModel>(filePath); // your errors will be stored in the same location as the application is run.
```

If you want to validate the csv file before reading it, you can use the `IsValid` method.

### Example

```csharp
   var result = csvCoreReader
       .IsValid<PersonModel>(filePath);
```

The `IsValid` method will return a `List<ValidationModel>` containing:
- The line number of the invalid record.
- The property name of the invalid record.
- The reason why the data could not be parsed stored in the error message property

This could be handy in numerous ways.

### Date/Time formats

This is going to be a slipery slope, but we will try to make it as easy as possible.
DateTime and/or DateOnly properties are a bit tricky, because we have to parse the data in the csv file to a DateTime or DateOnly object.

We gave it a try, and we think we did a good job.

If you have a date/time format that is not the default one, you can set the format using the `SetDateTimeFormat` method.
```csharp
    var result = csvCoreReader
        .SetDateTimeFormat("dd/MM/yyyy") // Set the date format to dd/MM/yyyy
        .Read<PersonModel>(filePath);
```
Be aware these options are always tricky so if you encounter any issues with it, please report an issue at GitHub with as much information as possible.
A unit test example would be awesome!
