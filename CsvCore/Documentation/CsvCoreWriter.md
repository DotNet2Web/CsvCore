## Using the CsvCoreWriter
If you only need to write a csv file, you can register the `CsvCoreWriter` in your IoC container:

```csharp
builder.Services.AddScoped<ICsvCoreWriter, CsvCoreWriter>();
```

Then you can use the `ICsvCoreWriter` in your code:

### Example

```csharp
// The model to write out to the file.
public class PersonModel
{
    public string Name { get; set; }

    public string Surname { get; set; }

    public string Birthdate { get; set; }

    public string Email { get; set; }
}

```csharp
public class Foo(ICsvCoreWriter csvCoreWriter)
{
    public void WriteUsingYourCultureSpecificDelimiter()
    {
         var records = new List<PersonModel>
         {
            new()
            {
                Name = "Foo",
                Surname = "Bar",
                BirthDate = new DateOnly(2025, 04, 16),
                Email = "foo@bar.nl"
            }
         };

         csvCoreWriter.Write(Path.Combine("AnyPath", "YourFile.csv"), records);
    }

    public void WriteWithoutHeaderRecordAndUseACustomDelimiter()
    {
         var records = new List<PersonModel>
         {
            new()
            {
                Name = "Foo",
                Surname = "Bar",
                BirthDate = new DateOnly(2025, 04, 16),
                Email = "foo@bar.nl"
            }
         };

         csvCoreWriter
           .UseDelimiter(';') // Specify your custom delimiter.
           .WithoutHeader()
           .Write<ResultModel>(Path.Combine("AnyLocation", "YourFile.csv"), records);
    }
}
```

Thats it, seems easy enough right?

In case you want to write your csv header different then the property names of you model, you can use the `Header` attribute in the model.
Using the `Header` attribute you can specify the name of the header column in the csv file.

CSV file
```text
First_Name;Family Name;DateOfBirth;Contact Email
Bar;Foo;01/01/2025;foo@bar.com
```

```csharp
public class NotMatchingPersonModel
{
    [Header(name: "First_Name")]
    public string Name { get; set; }

    [Header(name: "Family Name"))
    public string Surname { get; set; }

    [Header(name: "DatOfBirth"))]
    public DateOnly BirthDate { get; set; }

    [Header(name: "Contact Email")]
    public string Email { get; set; }
}
```

When you have the model all setup use the `CsvCoreWriter` to write the csv file, see the above section for the example.
