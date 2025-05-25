using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using CsvCore.Reader;
using CsvCore.Specs.Helpers;
using CsvCore.Specs.Models;
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Specs.Models.Enums;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Features.Reading.Conversion;

public class CsvCoreReaderConversionSpecs
{
    private const string CsvExtension = "csv";

    [Fact]
    public async Task Should_Read_Provided_Csv_File_With_Header_When_A_DateTime_Is_Available_In_The_Data_And_Model()
    {
        // Arrange
        var dateFormat = "yyyyMMddTHHmmss";

        var csvCoreReader = new CsvCoreReader();

        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        await File.Create(filePath).DisposeAsync();

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

        await File.WriteAllTextAsync(filePath, content);

        // Act
        var result = await csvCoreReader
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

    [Fact]
    public async Task Should_Read_Provided_Csv_File_When_The_Result_Model_Contains_A_Guid()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        await File.Create(filePath).DisposeAsync();

        var cars = new Faker<CsvCarContentModel>()
            .RuleFor(c => c.Id, faker => faker.Vehicle.Random.Guid().ToString())
            .RuleFor(c => c.Manufacturer, faker => faker.Vehicle.Manufacturer().ToString())
            .RuleFor(c => c.Model, faker => faker.Vehicle.Model().ToString())
            .RuleFor(c => c.Vin, faker => faker.Vehicle.Vin().ToString())
            .RuleFor(c => c.YearOfConstruction, faker => faker.Date.Past().Year.ToString())
            .RuleFor(c => c.Mileage, faker => faker.Vehicle.Random.Int().ToString())
            .RuleFor(c => c.Fuel, faker => faker.Vehicle.Fuel().ToString())
            .Generate(2);

        var csvCoreWriter = new CsvCoreWriter();
        csvCoreWriter
            .UseDelimiter(';')
            .Write(filePath, cars);

        // Act
        var result = await csvCoreReader
            .UseDelimiter(';')
            .Read<CarResultModel>(filePath);

        // Assert
        var convertedCars = result.ToList();

        convertedCars.Count.Should().Be(2);

        convertedCars.First().Id.Should().Be(cars[0].Id);
        convertedCars.First().Manufacturer.Should().Be(cars[0].Manufacturer);
        convertedCars.First().Model.Should().Be(cars[0].Model);
        convertedCars.First().Vin.Should().Be(cars[0].Vin);
        convertedCars.First().Mileage.Should().Be(int.Parse(cars[0].Mileage));
        convertedCars.First().YearOfConstruction.Should().Be(int.Parse(cars[0].YearOfConstruction));

        convertedCars.Last().Id.Should().Be(cars[1].Id);
        convertedCars.Last().Manufacturer.Should().Be(cars[1].Manufacturer);
        convertedCars.Last().Model.Should().Be(cars[1].Model);
        convertedCars.Last().Vin.Should().Be(cars[1].Vin);
        convertedCars.Last().Mileage.Should().Be(int.Parse(cars[1].Mileage));
        convertedCars.Last().YearOfConstruction.Should().Be(int.Parse(cars[1].YearOfConstruction));

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Read_Provided_Csv_File_When_The_Result_Model_Contains_An_Enum()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        await File.Create(filePath).DisposeAsync();

        var cars = new Faker<CsvCarContentModel>()
            .RuleFor(c => c.Id, faker => faker.Vehicle.Random.Guid().ToString())
            .RuleFor(c => c.Manufacturer, faker => faker.Vehicle.Manufacturer().ToString())
            .RuleFor(c => c.Model, faker => faker.Vehicle.Model().ToString())
            .RuleFor(c => c.Vin, faker => faker.Vehicle.Vin().ToString())
            .RuleFor(c => c.YearOfConstruction, faker => faker.Date.Past().Year.ToString())
            .RuleFor(c => c.Mileage, faker => faker.Vehicle.Random.Int().ToString())
            .RuleFor(c => c.Fuel, faker => faker.Vehicle.Fuel().ToString())
            .Generate(2);

        var csvCoreWriter = new CsvCoreWriter();
        csvCoreWriter
            .UseDelimiter(';')
            .Write(filePath, cars);

        // Act
        var result = await csvCoreReader
            .UseDelimiter(';')
            .Read<CarResultModel>(filePath);

        // Assert
        var convertedCars = result.ToList();
        convertedCars.Count.Should().Be(2);

        convertedCars.First().Id.Should().Be(cars[0].Id);
        convertedCars.First().Fuel.ToString().Should().Be(cars[0].Fuel);

        convertedCars.Last().Id.Should().Be(cars[1].Id);
        convertedCars.Last().Fuel.ToString().Should().Be(cars[1].Fuel);

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Read_Provided_Csv_File_When_The_Result_Model_Contains_An_IntValue_For_The_Enum()
    {
        // Arrange
        var csvCoreReader = new CsvCoreReader();
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        await File.Create(filePath).DisposeAsync();

        var cars = new Faker<CsvCarContentModel>()
            .RuleFor(c => c.Id, faker => faker.Vehicle.Random.Guid().ToString())
            .RuleFor(c => c.Manufacturer, faker => faker.Vehicle.Manufacturer().ToString())
            .RuleFor(c => c.Model, faker => faker.Vehicle.Model().ToString())
            .RuleFor(c => c.Vin, faker => faker.Vehicle.Vin().ToString())
            .RuleFor(c => c.YearOfConstruction, faker => faker.Date.Past().Year.ToString())
            .RuleFor(c => c.Mileage, faker => faker.Vehicle.Random.Int().ToString())
            .RuleFor(c => c.Fuel, faker => faker.Random.Int(0, 3).ToString())
            .Generate(2);

        var csvCoreWriter = new CsvCoreWriter();
        csvCoreWriter
            .UseDelimiter(';')
            .Write(filePath, cars);

        // Act
        var result = await csvCoreReader
            .UseDelimiter(';')
            .Read<CarResultModel>(filePath);

        // Assert
        var convertedCars = result.ToList();
        convertedCars.Count.Should().Be(2);

        convertedCars.First().Id.Should().Be(cars[0].Id);
        convertedCars.First().Fuel.Should().Be(Enum.Parse<Fuel>(cars[0].Fuel));

        convertedCars.Last().Id.Should().Be(cars[1].Id);
        convertedCars.Last().Fuel.Should().Be(Enum.Parse<Fuel>(cars[1].Fuel));

        // Cleanup
        FileHelper.DeleteTestFile(filePath);
    }
}
