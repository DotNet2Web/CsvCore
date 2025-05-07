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
using CsvCore.Writer;
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
    public void Should_Throw_MissingFileException_When_Trying_To_Validate_The_Input_File()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();

        // Act
        var act = () => csvCoreReader
            .IsValid<PersonModel>(Path.Combine(Directory.GetCurrentDirectory(), new Faker().System.FileName(CsvExtension)));

        // Assert
        act.Should().Throw<MissingFileException>();
    }

    [Fact]
    public void Should_Validate_The_Input_That_Only_Contains_Valid_Data()
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
        var result = csvCoreReader
            .WithoutHeader()
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
        var result = csvCoreReader
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
    public void
        Should_Read_Provided_Csv_File_Without_Header_And_Still_Set_The_Data_On_The_Correct_Properties_Even_With_A_ZeroBasedModel()
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

    [Fact]
    public void Should_Validate_The_Input_When_Reading_The_Csv_File()
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
        var result = csvCoreReader
            .Read<PersonModel>(filePath).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(10);

        var errorFile = Path.GetFileNameWithoutExtension(filePath);
        var errorFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Errors");

        var errors = File.ReadAllLines(Path.Combine(errorFolderPath, $"{errorFile}_errors.csv"));
        errors.Should().NotBeNull();
        errors.Length.Should().Be(4);

        errors[1].Should().Be($"6{delimiter}BirthDate{delimiter}Cannot convert '01-01-2023T00:00:00' to System.DateOnly.");
        errors[2].Should().Be($"7{delimiter}Name{delimiter}The value for Name cannot be null or empty.");
        errors[3].Should().Be($"7{delimiter}Email{delimiter}The value for Email cannot be null or empty.");

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
        FileHelper.DeleteTestFile(errorFolderPath);
    }

    [Fact]
    public void Should_Write_An_ErrorFile_Without_Using_The_WriteErrorsAt_Method()
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
        var result = csvCoreReader
            .Read<PersonModel>(filePath).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(5);

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
    public void Should_Validate_The_Input_When_Reading_The_Csv_File_And_Write_An_ErrorFile_To_The_Provided_Location(
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
        var result = csvCoreReader
            .SetErrorPath(errorLocation)
            .Read<PersonModel>(filePath).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(5);

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
    public void Should_Read_Provided_Csv_File_With_Header_When_A_DateTime_Is_Available_In_The_Data_And_Model()
    {
        // Arrange
        var dateFormat = "yyyyMMddTHHmmss";

        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentDateTimeModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.CreatedOn, faker => faker.Date.Future().ToString(dateFormat))
            .RuleFor(person => person.CreatedOn, faker => faker.Date.Future().ToString(dateFormat))
            .Generate(5);

        var contentBuilder = new StringBuilder();

        contentBuilder.AppendLine("Name;CreatedOn;ModifiedOn");

        foreach (var person in persons)
        {
            contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"{person.Name};{person.CreatedOn}");
        }

        var content = contentBuilder.ToString();

        File.WriteAllText(filePath, content);

        // Act
        var result = csvCoreReader
            .UseDelimiter(';')
            .SetDateTimeFormat(dateFormat)
            .Read<PersonCreatedModel>(filePath);

        // Assert
        var convertedPersons = result.ToList();

        convertedPersons.Count.Should().Be(5);

        foreach (var person in persons)
        {
            convertedPersons.SingleOrDefault(cvp => cvp.Name == person.Name).Should().NotBeNull();
            var convertedPerson = convertedPersons.Single(cvp => cvp.Name == person.Name);

            convertedPerson.CreatedOn.Should().Be(DateTime.ParseExact(person.CreatedOn, dateFormat,
                DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None));
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
    public void Should_Generate_A_Full_Model_With_Invalid_Records_When_Reading_The_Csv_File_Using_SkipValidation(
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
        var result = csvCoreReader
            .SkipValidation()
            .Read<PersonModel>(filePath).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().Be(12);

        result[5].BirthDate.Should().Be(DateOnly.MinValue);

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
