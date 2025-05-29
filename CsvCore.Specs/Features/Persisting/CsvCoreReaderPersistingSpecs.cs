using System;
using System.IO;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Reader;
using CsvCore.Specs.Db;
using CsvCore.Specs.Db.Entities;
using CsvCore.Specs.Extensions;
using CsvCore.Writer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CsvCore.Specs.Features.Persisting;

public class CsvCoreReaderPersistingSpecs
{
    private readonly TestDbContext _dbContext;

    private const string CsvExtension = "csv";
    private const char CustomDelimiter = ';';
    private const string TestDatabaseName = "TestDatabase";

    public CsvCoreReaderPersistingSpecs()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"{TestDatabaseName}_{Guid.NewGuid()}")
            .Options;

        _dbContext = new TestDbContext(options);
    }

    [Fact]
    public void Should_Throw_DbContextNotSetException()
    {
        // Arrange
        var reader = new CsvCoreReader();

        // Act
        var act = () => reader.Persist<Employee>("test.csv");

        // Assert
        act.Should().Throw<DbContextNotSetException>()
            .WithMessage("DbContext is not set. Use 'UseDbContext' method to set the DbContext.");
    }

    [Fact]
    public void Should_Add_New_Records_Into_DbSet_When_No_Records_Exists()
    {
        // Arrange
        var filePath = GenerateTestFile(5);

        _dbContext.Database.EnsureCreated();
        _dbContext.Database.EnsureDeleted();

        var reader = new CsvCoreReader();

        // Act
        reader
            .UseDelimiter(CustomDelimiter)
            .UseDbContext(_dbContext)
            .Persist<Employee>(filePath);

        // Assert
        _dbContext.Employees.Should().HaveCount(5);
    }

    [Fact]
    public void Should_Only_Add_New_Records_Into_DbSet_When_Some_Records_Already_Exists()
    {
        // Arrange
        var filePath = GenerateTestFile(5, true);
        var reader = new CsvCoreReader();

        // Act
        reader
            .UseDelimiter(CustomDelimiter)
            .UseDbContext(_dbContext)
            .Persist<Employee>(filePath);

        // Assert
        _dbContext.Employees.Should().HaveCount(5);
    }

    private string GenerateTestFile(int amountOfRecords, bool addToDb = false)
    {
        var directory = Directory.GetCurrentDirectory();

        var filePath = Path.Combine(directory, new Faker().System.FileName(CsvExtension));

        var persons = new Faker<Employee>()
            .RuleFor(person => person.Name, faker => faker.Person.FirstName)
            .RuleFor(person => person.Surname, faker => faker.Person.LastName)
            .RuleFor(person => person.BirthDate, faker => faker.Person.RandomDateOfBirth())
            .RuleFor(person => person.Email, faker => faker.Internet.Email())
            .Generate(amountOfRecords);

        new CsvCoreWriter()
            .UseDelimiter(CustomDelimiter)
            .Write(filePath, persons);

        if (!addToDb)
        {
            return filePath;
        }

        _dbContext.Employees.AddRange(persons.GetRange(0, 2));
        _dbContext.SaveChanges();

        return filePath;
    }
}
