using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using CsvCore.Models;
using CsvCore.Reader;
using CsvCore.Specs.Helpers;
using CsvCore.Specs.Models;
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Features.Reading.ErrorHandling;

public class CsvCoreReaderErrorHandlingSpecs
{
    private const string CsvExtension = "csv";

    [Fact]
    public async Task Should_Write_An_ErrorFile_Without_Setting_A_ErrorPath()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var delimiter = char.Parse(CultureInfo.CurrentCulture.TextInfo.ListSeparator);

        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        var invalid = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        invalid.BirthDate = "01-01-2023T00:00:00";
        persons.Add(invalid);

        new CsvCoreWriter()
            .UseDelimiter(delimiter)
            .Write(filePath, persons);

        // Act
        var result = await csvCoreReader
            .Validate()
            .Read<PersonModel>(filePath);

        // Assert
        var resultList = result.ToList();

        resultList.Should().NotBeEmpty();
        resultList.Count.Should().Be(5);

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
        var errorFile = Path.GetFileNameWithoutExtension(filePath);
        var errors = File.ReadAllLines(Path.Combine(path, $"{errorFile}_errors.csv"));

        errors.Should().NotBeNull();
        errors[1].Should().Be($"6{delimiter}BirthDate{delimiter}Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
        FileHelper.DeleteTestFile(errorFile);
    }

    [Theory]
    [InlineData(@"C:\Temp\Errors", false)]
    [InlineData("", false)]
    [InlineData(@"C:\Temp\Errors", true)]
    [InlineData("", true)]
    public async Task Should_Validate_The_Input_When_Reading_The_Csv_File_And_Write_An_ErrorFile_To_The_Provided_Location(
        string errorLocation, bool withoutHeader)
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var delimiter = char.Parse(CultureInfo.CurrentCulture.TextInfo.ListSeparator);

        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        var invalid = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        invalid.BirthDate = "01-01-2023T00:00:00";
        persons.Add(invalid);

        var csvCoreWriter = new CsvCoreWriter();

        if (withoutHeader)
        {
            csvCoreWriter.WithoutHeader();
            csvCoreReader.WithoutHeader();
        }

        csvCoreWriter.UseDelimiter(delimiter)
            .Write(filePath, persons);

        // Act
        var result = await csvCoreReader
            .Validate(errorLocation)
            .Read<PersonModel>(filePath);

        // Assert
        var resultList = result.ToList();

        resultList.Should().NotBeEmpty();
        resultList.Count.Should().Be(5);

        var errorFile = Path.GetFileNameWithoutExtension(filePath);

        if (string.IsNullOrEmpty(errorLocation))
        {
            errorLocation = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
        }

        var errors = File.ReadAllLines(Path.Combine(errorLocation, $"{errorFile}_errors.csv"));

        errors.Should().NotBeNull();
        errors[1].Should().Be($"6{delimiter}BirthDate{delimiter}Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
        FileHelper.DeleteTestFile(errorFile);
    }

    [Fact]
    public async Task Should_Add_All_Missing_Required_Fields_Into_Error_File_When_Data_Is_Missing()
    {
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var cars = new Faker<CsvCarContentModel>()
            .RuleFor(c => c.Id, faker => faker.Vehicle.Random.Guid().ToString())
            .RuleFor(c => c.Manufacturer, faker => faker.Vehicle.Manufacturer().ToString())
            .RuleFor(c => c.Model, faker => faker.Vehicle.Model().ToString())
            .RuleFor(c => c.Vin, faker => faker.Vehicle.Vin().ToString())
            .RuleFor(c => c.YearOfConstruction, _ => null)
            .RuleFor(c => c.Mileage, _ => null)
            .RuleFor(c => c.Fuel, faker => faker.Vehicle.Fuel().ToString())
            .Generate(2);

        var csvCoreWriter = new CsvCoreWriter();

        csvCoreWriter
            .UseDelimiter(';')
            .Write(filePath, cars);

        // Act
        var result = await csvCoreReader
            .UseDelimiter(';')
            .Validate()
            .Read<CarResultModel>(filePath);

        // Assert
        var convertedCars = result.ToList();
        convertedCars.Count.Should().Be(0);

        var errorFile = Path.GetFileNameWithoutExtension(filePath);
        var errorFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
        var errors = await csvCoreReader.UseDelimiter(';')
            .Read<ValidationModel>(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"));

        var groupedErrors = errors.ToList().GroupBy(e => e.RowNumber).ToList();

        foreach (var error in groupedErrors)
        {
            error.Count().Should().Be(2);
            error.First().PropertyName.Should().Be("YearOfConstruction");
            error.Last().PropertyName.Should().Be("Mileage");
        }

        groupedErrors.Count.Should().Be(2);

        groupedErrors[0].First().RowNumber.Should().Be(1);
        groupedErrors[1].First().RowNumber.Should().Be(2);

        groupedErrors[0].First().ConversionError.Should().Be("The value for YearOfConstruction cannot be null or empty.");
        groupedErrors[0].Last().ConversionError.Should().Be("The value for Mileage cannot be null or empty.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
