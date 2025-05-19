using System.IO;
using System.Threading.Tasks;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Reader;
using CsvCore.Specs.Db;
using CsvCore.Specs.Db.Entities;
using CsvCore.Specs.Extensions;
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Writer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CsvCore.Specs.Features.Persisting;

public class CsvCoreReaderPersistingSpecs
{
    private const string CsvExtension = "csv";
    private const char CustomDelimiter = ';';

    [Fact]
    public async Task Should_Throw_DbContextNotSetException()
    {
        // Arrange
        var reader = new CsvCoreReader();

        // Act
        var act = () => reader.Persist<Employee>("test.csv");

        // Assert
        await act.Should().ThrowAsync<DbContextNotSetException>()
            .WithMessage("DbContext is not set. Use 'UseDbContext' method to set the DbContext.");
    }

    [Fact]
    public async Task Should_Add_New_Records_Into_DbSet_When_No_Records_Exists()
    {
        // Arrange
        var filePath = GenerateTestFile(5);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;

        var dbContext = new TestDbContext(options);

        var reader = new CsvCoreReader();

        // Act
        await reader
            .UseDbContext(dbContext)
            .Persist<Employee>(filePath);

        // Assert
        dbContext.Employees.Should().HaveCount(5);
    }

    private string GenerateTestFile(int amountOfRecords)
    {
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        File.Create(filePath).Dispose();

        var persons = new Faker<CsvContentModel>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.RandomDateOfBirth().ToString())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(amountOfRecords);

        new CsvCoreWriter()
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        return filePath;
    }
}
