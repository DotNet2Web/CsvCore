using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Reader;
using CsvCore.Specs.Helpers;
using CsvCore.Specs.Models;
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Features.Validation;

public class CsvCoreReaderValidationAsyncSpecs
{
    private const string CsvExtension = "csv";
    private const string ErrorsPath = "Errors";

    [Fact]
    public async Task Should_Throw_MissingFileException_When_Trying_To_Validate_The_Input_File()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        // Act
        var act = async () => await csvCoreReader
            .IsValidAsync<PersonModel>(Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension)));

        // Assert
        await act.Should().ThrowAsync<MissingFileException>();
    }

    [Fact]
    public async Task Should_Validate_The_Input_That_Only_Contains_Valid_Data()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));
        var delimiter = char.Parse(CultureInfo.CurrentCulture.TextInfo.ListSeparator);

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        new CsvCoreWriter().UseDelimiter(delimiter).Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = await csvCoreReader
            .WithoutHeader()
            .IsValidAsync<PersonModel>(filePath);

        // Assert
        result.Should().BeEmpty();

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Validate_The_Input_When_A_Date_Cannot_Be_Converted()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        var delimiter = char.Parse(CultureInfo.CurrentCulture.TextInfo.ListSeparator);

        var invalid = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        invalid.BirthDate = "01-01-2023T00:00:00";

        persons.Add(invalid);

        new CsvCoreWriter().UseDelimiter(delimiter)
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = await csvCoreReader
            .IsValidAsync<PersonModel>(filePath);

        // Assert
        var resultList = result.ToList();

        resultList.Should().NotBeEmpty();
        resultList.Count.Should().Be(1);

        resultList.First().RowNumber.Should().Be(6);
        resultList.First().PropertyName.Should().Be("BirthDate");
        resultList.First().ConversionError.Should().Be("Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Validate_The_Input_When_Reading_The_Csv_File()
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

        var invalid1 = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        invalid1.BirthDate = "01-01-2023T00:00:00";

        var invalid2 = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, _ => null)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, _ => null)
            .Generate();

        var anotherSetValidData = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        persons.Add(invalid1);
        persons.Add(invalid2);
        persons.AddRange(anotherSetValidData);

        new CsvCoreWriter().UseDelimiter(delimiter).Write(filePath, persons);

        // Act
        var result = await csvCoreReader
            .Validate()
            .ReadAsync<PersonModel>(filePath);

        // Assert
        var resultList = result.ToList();
        resultList.Should().NotBeEmpty();
        resultList.Count.Should().Be(10);

        var errorFile = Path.GetFileNameWithoutExtension(filePath);
        var errorFolderPath = Path.Combine(Directory.GetCurrentDirectory(), ErrorsPath);

        var errors = await File.ReadAllLinesAsync(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"));
        errors.Should().NotBeNull();
        errors.Length.Should().Be(4);

        errors[1].Should().Be($"6{delimiter}BirthDate{delimiter}Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");
        errors[2].Should().Be($"7{delimiter}Name{delimiter}The value for Name cannot be null or empty.");
        errors[3].Should().Be($"7{delimiter}Email{delimiter}The value for Email cannot be null or empty.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
        FileHelper.DeleteTestFile(errorFolderPath);
    }

    [Theory]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData(" ", true)]
    [InlineData("", true)]
    [InlineData(null, true)]
    public async Task Should_Generate_A_Full_Model_With_Invalid_Records_When_Reading_The_Csv_File_Without_Validation(
        string invalidBirthDate, bool withoutHeader)
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

        var invalid1 = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, _ => invalidBirthDate)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        var invalid2 = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, _ => null)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, _ => null)
            .Generate();

        var anotherSetValidData = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        persons.Add(invalid1);
        persons.Add(invalid2);
        persons.AddRange(anotherSetValidData);

        var csvCoreWriter = new CsvCoreWriter();

        if (withoutHeader)
        {
            csvCoreWriter.WithoutHeader();
        }

        csvCoreWriter.UseDelimiter(delimiter).Write(filePath, persons);

        if (withoutHeader)
        {
            csvCoreReader.WithoutHeader();
        }

        // Act
        var result = await csvCoreReader
            .ReadAsync<PersonModel>(filePath);

        // Assert
        var resultList = result.ToList();
        resultList.Should().NotBeEmpty();
        resultList.Count.Should().Be(12);

        resultList[5].BirthDate.Should().Be(DateOnly.MinValue);

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
