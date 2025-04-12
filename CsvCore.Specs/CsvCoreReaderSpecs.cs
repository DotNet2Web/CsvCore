using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Reader;
using CsvCore.Specs.Models;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs;

public class CsvCoreReaderSpecs
{
    private readonly CsvCoreReader csvHelper = new();

    [Theory]
    [InlineData("")]
    [InlineData("test.csv")]
    public void Should_Throw_MissingFileException_When_FilePath_Is_Null(string filePath)
    {
        // Act
        var act = () => csvHelper
            .ForFile(filePath)
            .Read<PersonSpec>();

        // Assert
        act.Should().Throw<MissingFileException>();
    }

    [Fact]
    public void Should_Read_Provided_Csv_File()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, "test.csv");

        File.Create(filePath).Dispose();

        var persons = new Faker<PersonSpec>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.Birthdate, (faker, _) => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(2);

        var contentBuilder = new StringBuilder();

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"{person.Name};{person.Surname};{person.Birthdate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .ForFile(filePath)
            .UseDelimiter(';')
            .Read<PersonSpec>();

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(2);
        convertedPersons.Should().BeEquivalentTo(persons);

        // Tear Down
        File.Delete(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, "test.csv");

        File.Create(filePath).Dispose();

        var persons = new Faker<PersonSpec>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.Birthdate, (faker, _) => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Name;Surname;Birthdate;Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"{person.Name};{person.Surname};{person.Birthdate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .ForFile(filePath)
            .UseDelimiter(';')
            .HasHeaderRecord()
            .Read<PersonSpec>();

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);
        convertedPersons.Should().BeEquivalentTo(persons);

        // Tear Down
        File.Delete(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header_Using_The_Region_Delimiter_Settings()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, "test.csv");

        File.Create(filePath).Dispose();

        var persons = new Faker<PersonSpec>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.Birthdate, (faker, _) => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();
        var delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        contentBuilder.AppendLine($"Name{delimiter}Surname{delimiter}Birthdate{delimiter}Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.CurrentCulture, $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.Birthdate}{delimiter}{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .ForFile(filePath)
            .HasHeaderRecord()
            .Read<PersonSpec>();

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);
        convertedPersons.Should().BeEquivalentTo(persons);

        // Tear Down
        File.Delete(filePath);
    }
}
