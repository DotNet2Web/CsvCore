using System;
using System.Collections.Generic;
using System.IO;
using CsvCore.Exceptions;
using CsvCore.Specs.Models;
using CsvCore.Writer;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs;

public class CsvCoreWriterSpecs : IDisposable
{
    [Fact]
    public void Should_Throw_ArgumentException_When_There_Are_No_Records_To_Write()
    {
        // Arrange
        var records = new List<PersonModel>();

        // Act
        var act = () => new CsvCoreWriter().Write("test.csv", records);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The records collection cannot be null or empty.");
    }

    [Fact]
    public void Should_Write_Csv_With_Records()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, "test.csv");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

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
        csvWriter.Write(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "test.csv"));
        fileContent.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Write_Csv_Without_Header()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, "test.csv");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

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
        csvWriter
            .WithoutHeader()
            .Write(filePath, records);

        // Assert
        var fileContent = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "test.csv"));
        fileContent.Should().HaveCount(1);
    }

    [Fact]
    public void Should_Throw_FileWritingException_When_The_Csv_File_Could_Not_Be_Written()
    {
        // Arrange
        var filePath = Path.Combine(Environment.CurrentDirectory, "test.csv");
        using var writer = new StreamWriter(filePath);

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

        // Act
        var act = () => new CsvCoreWriter().Write(filePath, records);

        // Assert
        act.Should().Throw<FileWritingException>().WithMessage($"Could not write the CSV file to {filePath}, please check the exception.");
    }

    public void Dispose()
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, "test.csv");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
