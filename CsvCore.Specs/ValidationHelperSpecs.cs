using Bogus;
using CsvCore.Helpers;
using CsvCore.Models;
using CsvCore.Specs.Models;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CsvCore.Specs;

public class ValidationHelperSpecs
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_ReturnNull_WhenValueIsEmptyAndPropertyIsNullable([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Id));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Return_ValidationModel_When_Value_Is_Null_But_Property_Is_Not_Nullable([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Name));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.Name));
        result.ConversionError.Should().Be("The value for Name cannot be null or empty.");
    }

    [Theory]
    [InlineData("2")]
    [InlineData("treu")]
    public void Should_Return_ValidationModel_When_Value_Cannot_Be_Converted_To_Boolean([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Active));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.Active));
        result.ConversionError.Should().Be($"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}.");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("true")]
    public void Should_Return_Null_When_Value_Can_Be_Converted_To_Boolean([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Active));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("two")]
    [InlineData("26.2")]
    public void Should_Return_ValidationModel_When_Value_Cannot_Be_Converted_To_An_Integer([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Id));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.Id));
        result.ConversionError.Should().Be($"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}.");
    }

    [Theory]
    [InlineData("-two")]
    public void Should_Return_ValidationModel_When_Value_Cannot_Be_Converted_To_A_Number([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Amount));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.Amount));
        result.ConversionError.Should().Be($"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}.");
    }

    [Theory]
    [InlineData("2323-25-12")]
    [InlineData("2323-25-12T12:12:12")]
    public void Should_Return_ValidationModel_When_Value_Cannot_Be_Converted_To_A_DateOnly([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.Date));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.Date));
        result.ConversionError.Should().Be($"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}.");
    }

    [Theory]
    [InlineData("2323-25-12")]
    [InlineData("2323-25-12T12:12:12")]
    public void Should_Return_ValidationModel_When_Value_Cannot_Be_Converted_To_A_DateTime([CanBeNull] string value)
    {
        // Arrange
        var validationHelper = new ValidationHelper();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.DateTime));

        // Act
        var result = validationHelper.Validate(value, property!, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ValidationModel>();

        result!.RowNumber.Should().Be(1);
        result.PropertyName.Should().Be(nameof(ValidationTestModel.DateTime));
        result.ConversionError.Should().Be($"Cannot convert '{value}' to {property.PropertyType.UnderlyingSystemType}.");
    }

    [Fact]
    public void Should_Return_An_Empty_ValidationList_When_Value_Can_Be_Converted_To_A_DateTime()
    {
        // Arrange
        var validationHelper = new ValidationHelper();
        var value = new Faker().Date.Future();

        var property = typeof(ValidationTestModel).GetProperty(nameof(ValidationTestModel.DateTime));

        // Act
        var result = validationHelper.Validate(value.ToString(), property!, 1);

        // Assert
        result.Should().BeNull();
    }
}
