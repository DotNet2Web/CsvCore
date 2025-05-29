using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bogus;
using CsvCore.Exceptions;
using CsvCore.Specs.Helpers;
using CsvCore.Specs.Models;
using CsvCore.Specs.Models.CsvContent;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Features.Writing;

public class CsvCoreWriterAsyncSpecs
{
    private const string CsvExtension = "csv";

    [Fact]
    public async Task Should_Throw_ArgumentException_When_There_Are_No_Records_To_Write()
    {
        // Arrange
        var records = new List<PersonModel>();

        // Act
        var act = async () => await new CsvCoreWriter().WriteAsync("test.csv", records);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("The records collection cannot be null or empty.");
    }

    [Fact]
    public async Task Should_Write_Csv_With_Records()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, new Faker().System.FileName(CsvExtension));

        var records = new List<PersonModel>
        {
            new() { Name = "Foo", Surname = "Bar", BirthDate = new DateOnly(2025, 04, 16), Email = "foo@bar.nl" }
        };

        var csvWriter = new CsvCoreWriter();

        // Act
        await csvWriter.WriteAsync(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(filePath);
        fileContent.Should().HaveCount(2);

        // Clean up
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Write_Csv_Without_Header()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, new Faker().System.FileName(CsvExtension));

        var records = new List<PersonModel>
        {
            new() { Name = "Foo", Surname = "Bar", BirthDate = new DateOnly(2025, 04, 16), Email = "foo@bar.nl" }
        };

        var csvWriter = new CsvCoreWriter();

        // Act
        await csvWriter
            .WithoutHeader()
            .WriteAsync(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(filePath);
        fileContent.Should().HaveCount(1);

        // Clean up
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Throw_FileWritingException_When_The_Csv_File_Could_Not_Be_Written()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, new Faker().System.FileName(CsvExtension));

        await using var writer = new StreamWriter(filePath);

        var records = new List<PersonModel>
        {
            new() { Name = "Foo", Surname = "Bar", BirthDate = new DateOnly(2025, 04, 16), Email = "foo@bar.nl" }
        };

        // Act
        var act = async () => await new CsvCoreWriter().WriteAsync(filePath, records);

        // Assert
        await act.Should().ThrowAsync<FileWritingException>()
            .WithMessage($"Could not write the CSV file to {filePath}, please check the exception.");
    }

    [Fact]
    public async Task Should_Write_Csv_With_Custom_Delimiter()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, new Faker().System.FileName(CsvExtension));

        var records = new List<PersonModel>
        {
            new()
            {
                Name = "Foo",
                Surname = "Bar",
                BirthDate = new DateOnly(2025, 04, 16),
                Email = "foo@bar.nl"
            }
        };

        var csvWriter = new CsvCoreWriter();

        // Act
        await csvWriter
            .UseDelimiter('|')
            .WriteAsync(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(filePath);
        fileContent.Should().HaveCount(2);

        fileContent[0].Should().Be("Name|Surname|BirthDate|Email");
        fileContent[1].Should().Contain("Foo|Bar|");

        // Clean up
        FileHelper.DeleteTestFile(filePath);
    }

    [Fact]
    public async Task Should_Write_Csv_With_Custom_Header_Names()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, new Faker().System.FileName(CsvExtension));

        var records = new List<CsvCustomHeaderModel>
        {
            new()
            {
                Name = "Foo",
                Surname = "Bar",
                BirthDate = new DateOnly(2025, 04, 16),
                Email = "foo@bar.nl"
            }
        };

        var csvWriter = new CsvCoreWriter();

        // Act
        await csvWriter
            .UseDelimiter('|')
            .WriteAsync(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(filePath);
        fileContent.Should().HaveCount(2);

        fileContent[0].Should().Be("Firstname|family_name|dateOfBirth|email contact");

        // Clean up
        FileHelper.DeleteTestFile(filePath);
    }
}
