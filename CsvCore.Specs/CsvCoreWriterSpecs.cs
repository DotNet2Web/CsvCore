using System;
using System.Collections.Generic;
using System.IO;
using CsvCore.Specs.Models;
using CsvCore.Writer;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CsvCore.Specs;

public class CsvCoreWriterSpecs
{
    [Theory]
    [InlineData(null)]
    public void Should_Throw_ArgumentException_When_There_Are_No_Records_To_Write([CanBeNull] IEnumerable<PersonModel> records)
    {
        // Act
        var act = () => new CsvCoreWriter().Write("test.csv", records);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The records collection cannot be null or empty.");
    }

    [Fact]
    public void Should_Write_Csv_With_Records()
    {
        // Arrange
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
            .Write(Path.Combine(Environment.CurrentDirectory, "test.csv"), records);

        // Assert
        var fileContent = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "test.csv"));
        fileContent.Should().HaveCount(2);
    }
}
