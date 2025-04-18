using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Reader;
using CsvCore.Specs.Extensions;
using CsvCore.Specs.Helpers;
using CsvCore.Specs.Models;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs;

public class CsvCoreReaderSpecs
{
    private const string CsvExtension = "csv";

    [Theory]
    [InlineData("")]
    [InlineData("test.csv")]
    public void Should_Throw_MissingFileException_When_FilePath_Is_Null(string filePath)
    {
        // Act
        var act = () => new CsvCoreReader()
            .Read<PersonModel>(filePath);

        // Assert
        act.Should().Throw<MissingFileException>();
    }

    [Fact]
    public void Should_Read_Provided_Csv_File()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(2);

        var contentBuilder = new StringBuilder();

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{person.Name};{person.Surname};{person.BirthDate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(2);

        foreach (var person in persons)
        {
            convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name).Should().NotBeNull();
            var convertedPerson = convertedPersons.Single(cvp => cvp.Name == person.Name);

            convertedPerson.Surname.Should().Be(person.Surname);
            convertedPerson.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Name;Surname;Birthdate;Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{person.Name};{person.Surname};{person.BirthDate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .HasHeaderRecord()
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name).Should().NotBeNull();
            var convertedPerson = convertedPersons.Single(cvp => cvp.Name == person.Name);

            convertedPerson.Surname.Should().Be(person.Surname);
            convertedPerson.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header_Using_The_Region_Delimiter_Settings()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();
        var delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        contentBuilder.AppendLine($"Name{delimiter}Surname{delimiter}Birthdate{delimiter}Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.CurrentCulture,
                $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.BirthDate}{delimiter}{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .HasHeaderRecord()
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name).Should().NotBeNull();
            var convertedPerson = convertedPersons.Single(cvp => cvp.Name == person.Name);

            convertedPerson.Surname.Should().Be(person.Surname);
            convertedPerson.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Validate_The_Input_That_Only_Contains_Valid_Data()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();
        var delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        contentBuilder.AppendLine($"Name{delimiter}Surname{delimiter}Birthdate{delimiter}Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.CurrentCulture,
                $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.BirthDate}{delimiter}{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .HasHeaderRecord()
            .IsValid<PersonModel>(filePath);

        // Assert
        result.Should().BeEmpty();

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Validate_The_Input_When_An_Date_Cannot_Be_Converted()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();
        var delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        contentBuilder.AppendLine($"Name{delimiter}Surname{delimiter}Birthdate{delimiter}Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.CurrentCulture,
                $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.BirthDate}{delimiter}{person.Email}");
        }

        var invalid = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        invalid.BirthDate = "01-01-2023T00:00:00";
        contentBuilder.AppendLine(CultureInfo.CurrentCulture,
            $"{invalid.Name}{delimiter}{invalid.Surname}{delimiter}{invalid.BirthDate}{delimiter}{invalid.Email}");

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .HasHeaderRecord()
            .IsValid<PersonModel>(filePath)
            .ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(1);

        result.First().RowNumber.Should().Be(6);
        result.First().PropertyName.Should().Be("BirthDate");
        result.First().ConversionError.Should().Be("Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Throw_MissingContentException_When_Another_Delimiter_Is_Used()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var person = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.DateOfBirth.ToShortDateString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate();

        var contentBuilder = new StringBuilder();
        var delimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        contentBuilder.AppendLine($"Name;Surname;Birthdate;Email");
        contentBuilder.AppendLine(CultureInfo.CurrentCulture,
            $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.BirthDate}{delimiter}{person.Email}");

        // Act
        var act = () => csvCoreReader
            .UseDelimiter(',')
            .Read<PersonModel>(filePath);

        // Assert
        act.Should().Throw<MissingContentException>()
            .WithMessage($"The file is empty, based on the ',' delimiter.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_Without_A_HeaderRecord()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Name;Surname;Birthdate;Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{person.Name};{person.Surname};{person.BirthDate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .HasHeaderRecord()
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name).Should().NotBeNull();
            var convertedPerson = convertedPersons.Single(cvp => cvp.Name == person.Name);

            convertedPerson.Surname.Should().Be(person.Surname);
            convertedPerson.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
