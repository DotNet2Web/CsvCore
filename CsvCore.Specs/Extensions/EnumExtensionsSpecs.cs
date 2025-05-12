using CsvCore.Extensions;
using CsvCore.Specs.Models;
using CsvCore.Specs.Models.Enums;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Extensions;

public class EnumExtensionsSpecs
{
    [Fact]
    public void ConvertToEnum_Returns_False_When_Trying_To_Convert_A_Number_To_Enum()
    {
        // Arrange
        var property = typeof(CarResultModel).GetProperty(nameof(CarResultModel.YearOfConstruction));
        var target = new CarResultModel();

        // Act
        var result = "2024".ConvertToEnum(property!, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertToEnum_Returns_False_When_Providing_Invalid_Enum()
    {
        // Arrange
        var property = typeof(CarResultModel).GetProperty(nameof(CarResultModel.Fuel));
        var target = new CarResultModel();

        // Act
        var result = "UnknownEnumValue".ConvertToEnum(property!, target);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Gasoline", Fuel.Gasoline)]
    [InlineData("Diesel", Fuel.Diesel)]
    [InlineData("Electric", Fuel.Electric)]
    [InlineData("Hydrogen", Fuel.Hydrogen)]
    [InlineData("Hybrid", Fuel.Hybrid)]
    [InlineData("0", Fuel.Gasoline)]
    [InlineData("1", Fuel.Diesel)]
    [InlineData("2", Fuel.Electric)]
    [InlineData("3", Fuel.Hydrogen)]
    [InlineData("4", Fuel.Hybrid)]
    public void ConvertToEnum_Returns_False_When_Providing_Invalid_Guids(string enumValue, Fuel expectedEnumValue)
    {
        // Arrange
        var property = typeof(CarResultModel).GetProperty(nameof(CarResultModel.Fuel));
        var target = new CarResultModel();

        // Act
        var result = enumValue.ConvertToEnum(property!, target);

        // Assert
        result.Should().BeTrue();
        target.Fuel.Should().Be(expectedEnumValue);
    }
}
