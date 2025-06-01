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
    private const char CustomDelimiter = ';';

    [Theory]
    [InlineData("")]
    [InlineData("test.csv")]
    public void Should_Throw_MissingFileException_When_FilePath_Is_Null(string filePath)
    {
        // Act
        var act = () => new CsvCoreReader().Read<PersonModel>(filePath);

        // Assert
        act.Should().Throw<MissingFileException>();
    }

    [Fact]
    public void Should_Read_Provided_Csv_File()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(2);

        new CsvCoreWriter()
            .WithoutHeader()
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
            .WithoutHeader()
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(2);

        foreach (var person in persons)
        {
            var convertedPerson = convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);
            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header()
    {
        // Arrange
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        new CsvCoreWriter()
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson = convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);
            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_When_Missing_A_Property_In_The_Header()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .RuleFor(person => person.Email, (faker, _) => faker.Phone.PhoneNumber())
            .Generate(5);

        new CsvCoreWriter()
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();
        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson = convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);
            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header_Using_The_Region_Delimiter_Settings()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(1);

        new CsvCoreWriter()
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader.Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(1);

        foreach (var person in persons)
        {
            var convertedPerson = convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);
            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_With_Header_When_Properties_Are_Decorated_With_Another_Column_Name()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, (faker, _) => faker.Person.FirstName)
            .RuleFor(person => person.Surname, (faker, _) => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, (faker, _) => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, (faker, _) => faker.Internet.Email())
            .Generate(5);

        new CsvCoreWriter().Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader.Read<PersonCustomNames>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson = convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);
            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
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
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
            .WithoutHeader()
            .Read<PersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson =
                convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);

            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void Should_Read_Provided_Csv_File_Without_Header_And_Still_Set_The_Data_On_The_Correct_Properties()
    {
        // Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension));

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
            .UseDelimiter(CustomDelimiter)
            .WithoutHeader()
            .Read<NotMatchingPersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson =
                convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);

            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public void
        Should_Read_Provided_Csv_File_Without_Header_And_Still_Set_The_Data_On_The_Correct_Properties_Even_With_A_ZeroBasedModel()
    {
        // Arrange
        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

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

        File.WriteAllTextAsync(filePath, content);

        var csvCoreReader = new CsvCoreReader();

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
            .WithoutHeader()
            .Validate()
            .Read<ZeroBasedNotMatchingPersonModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            var convertedPerson =
                convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name && cvp.Surname == person.Surname);

            convertedPerson.Should().NotBeNull();

            convertedPerson!.Surname.Should().Be(person.Surname);
            convertedPerson!.BirthDate.Should().Be(DateOnly.Parse(person.BirthDate));
            convertedPerson!.Email.Should().Be(person.Email);
        }

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
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, cars);

        // Act
        var result = csvCoreReader
            .UseDelimiter(CustomDelimiter)
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

    [Fact]
    public void Should_Read_Provided_Csv_File_When_The_Result_Model_Contains_A_Complex_Type()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        var csvCompanies = new Faker<CsvCompanyContentModel>()
            .RuleFor(c => c.Name, faker => faker.Company.CompanyName().ToString())
            .RuleFor(c => c.ChamberOfCommerceNumber, faker => faker.Random.Int(0, 1_000).ToString())
            .RuleFor(c => c.Street, faker => faker.Address.StreetName().ToString())
            .RuleFor(c => c.HouseNumber, faker => faker.Address.BuildingNumber().ToString())
            .RuleFor(c => c.HouseNumberAddition, faker => faker.Address.SecondaryAddress().ToString())
            .RuleFor(c => c.Zipcode, faker => faker.Address.ZipCode().ToString())
            .RuleFor(c => c.City, faker => faker.Address.City().ToString())
            .Generate(1);

        var csvCoreWriter = new CsvCoreWriter();
        csvCoreWriter
            .Write(filePath, csvCompanies);

        // Act
        var result = csvCoreReader.Read<CompanyModel>(filePath);

        // Assert
        var companies = result.ToList();
        companies.Count.Should().Be(1);

        companies.First().Name.Should().Be(csvCompanies[0].Name);
        companies.First().ChamberOfCommerceNumber.Should().Be(int.Parse(csvCompanies[0].ChamberOfCommerceNumber));

        companies.First().Adddress.Should().NotBeNull();
        companies.First().Adddress.City.Should().Be(csvCompanies[0].City);
        companies.First().Adddress.Zipcode.Should().Be(csvCompanies[0].Zipcode);
        companies.First().Adddress.HouseNumber.Should().Be(int.Parse(csvCompanies[0].HouseNumber));
        companies.First().Adddress.HouseNumberAddition.Should().Be(csvCompanies[0].HouseNumberAddition);
        companies.First().Adddress.Street.Should().Be(csvCompanies[0].Street);

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
