using System;
using CsvCore.Extensions;
using CsvCore.Specs.Models;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Extensions;

public class DateTypeExtensionsSpecs
{
    [Theory]
    [InlineData("2025-10-01", "yyyy-MM-dd")]
    [InlineData("01-10-2025", "dd-MM-yyyy")]
    [InlineData("10/01/2025", "MM/dd/yyyy")]
    [InlineData("20251001", "yyyyMMdd")]
    [InlineData("2025-10-01", "")]
    public void Should_ConvertToDateOnly_When_DateOnlyType(string input, string dateFormat)
    {
        // Arrange
        var dateOnlyProperty = typeof(DateTypesTestModel)
            .GetProperty(nameof(DateTypesTestModel.DateOnlyProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = input.ConvertToDateTypes(dateFormat, dateOnlyProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.DateOnlyProperty.Should().Be(new DateOnly(2025, 10, 01));
    }

    [Theory]
    [InlineData("2025-10-01T19:26:26", "yyyy-MM-ddTHH:mm:ss")]
    [InlineData("01-10-2025T19:26:26", "dd-MM-yyyyTHH:mm:ss")]
    [InlineData("2025-10-01T19:26:26", "")]
    public void Should_ConvertToDateTime(string input, string dateFormat)
    {
        // Arrange
        var dateTimeProperty = typeof(DateTypesTestModel).GetProperty(nameof(DateTypesTestModel.DateTimeProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = input.ConvertToDateTypes(dateFormat, dateTimeProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.DateTimeProperty.Should().Be(new DateTime(2025, 10, 1, 19, 26, 26));
    }

    [Fact]
    public void Should_Convert_To_DateOnly_MinValue_When_Value_Is_Empty()
    {
        // Arrange
        var dateTimeProperty = typeof(DateTypesTestModel).GetProperty(nameof(DateTypesTestModel.DateOnlyProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = "".ConvertToDateTypes(null, dateTimeProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.DateOnlyProperty.Should().Be(DateOnly.MinValue);
    }

    [Fact]
    public void Should_Convert_To_DateTime_MinValue_When_Value_Is_Empty()
    {
        // Arrange
        var dateTimeProperty = typeof(DateTypesTestModel).GetProperty(nameof(DateTypesTestModel.DateTimeProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = "".ConvertToDateTypes(null, dateTimeProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.DateTimeProperty.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void Should_ConvertToDateTime_With_A_Nullable_DateOnly()
    {
        // Arrange
        var dateTimeProperty = typeof(DateTypesTestModel).GetProperty(nameof(DateTypesTestModel.NullableDateOnlyProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = "".ConvertToDateTypes(null, dateTimeProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.NullableDateOnlyProperty.Should().Be(null);
    }

    [Fact]
    public void Should_ConvertToDateTime_With_A_Nullable_DateTime()
    {
        // Arrange
        var dateTimeProperty = typeof(DateTypesTestModel).GetProperty(nameof(DateTypesTestModel.NullableDateTimeProperty));
        var target = new DateTypesTestModel();

        // Act
        var result = "".ConvertToDateTypes(null, dateTimeProperty!, target);

        // Assert
        result.Should().BeTrue();
        target.NullableDateTimeProperty.Should().Be(null);
    }
}
