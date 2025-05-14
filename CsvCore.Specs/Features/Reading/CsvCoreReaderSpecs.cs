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
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Features.Reading;

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
            .WithoutHeader()
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
    public void Should_Read_Provided_Csv_File_When_Missing_A_Property_In_The_Header()
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
            .RuleFor(person => person.Email, (faker, _) => faker.Phone.PhoneNumber())
            .Generate(5);

        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Name;Surname;Birthdate;Email;Phone");

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
            .Generate(1);

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
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(1);

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
    public void Should_Read_Provided_Csv_File_With_Header_When_Properties_Are_Decorated_With_Another_Column_Name()
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

        contentBuilder.AppendLine($"First_Name{delimiter}Family_Name{delimiter}Date_Of_Birth{delimiter}Contact_Email");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.CurrentCulture,
                $"{person.Name}{delimiter}{person.Surname}{delimiter}{person.BirthDate}{delimiter}{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .Read<PersonCustomNames>(filePath);

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
    public void Should_Read_Provided_Csv_File_Without_Header()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        new CsvCoreWriter()
            .WithoutHeader()
            .UseDelimiter(';')
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .WithoutHeader()
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
    public void Should_Read_Provided_Csv_File_Without_Header_And_Still_Set_The_Data_On_The_Correct_Properties()
    {
        // Arrange
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

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{person.Surname};{person.Name};{person.BirthDate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .WithoutHeader()
            .Read<NotMatchingPersonModel>(filePath);

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
    public void Should_Read_Provided_Csv_File_Without_Header_And_Still_Set_The_Data_On_The_Correct_Properties_Even_With_A_ZeroBasedModel()
    {
        // Arrange
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

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture,
                $"{person.Surname};{person.Name};{person.BirthDate};{person.Email}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .WithoutHeader()
            .Validate()
            .Read<ZeroBasedNotMatchingPersonModel>(filePath);

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

    [Theory]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData(" ", true)]
    [InlineData("", true)]
    [InlineData(null, true)]
    public void Should_Generate_A_Full_Model_With_Invalid_Records_When_Reading_The_Csv_File_Without_Validation(string invalidBirthDate, bool withoutHeader)
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
        var result = csvCoreReader
            .Read<PersonModel>(filePath).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(12);

        result[5].BirthDate.Should().Be(DateOnly.MinValue);

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_When_The_Result_Model_Contains_Only_A_Set_Of_Properties()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var cars = new Faker<CsvCarContentModel>()
            .RuleFor(c => c.Id, faker => faker.Vehicle.Random.Guid().ToString())
            .RuleFor(c => c.Manufacturer, faker => faker.Vehicle.Manufacturer().ToString())
            .RuleFor(c => c.Model, faker => faker.Vehicle.Model().ToString())
            .RuleFor(c => c.Vin, faker => faker.Vehicle.Vin().ToString())
            .RuleFor(c => c.YearOfConstruction, faker => faker.Date.Past().Year.ToString())
            .RuleFor(c => c.Mileage, faker => faker.Vehicle.Random.Int(0, 100_000).ToString())
            .RuleFor(c => c.Fuel, faker => faker.Random.Int(0, 3).ToString())
            .Generate(1);

        var csvCoreWriter = new CsvCoreWriter();
        csvCoreWriter
            .UseDelimiter(';')
            .Write(filePath, cars);

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .Read<CarWithOnlyAFewColumnsSelectedModel>(filePath);

        // Assert
        var convertedCar = result.ToList();
        convertedCar.Count.Should().Be(1);

        convertedCar.First().Id.Should().Be(cars[0].Id);
        convertedCar.First().Manufacturer.Should().Be(cars[0].Manufacturer);
        convertedCar.First().Model.Should().Be(cars[0].Model);
        convertedCar.First().Mileage.Should().Be(int.Parse(cars[0].Mileage));

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
