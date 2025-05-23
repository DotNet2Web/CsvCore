using System;
using CsvCore.Extensions;
using CsvCore.Specs.Models;
using FluentAssertions;
using Xunit;

namespace CsvCore.Specs.Extensions;

public class GuidExtensionsSpecs
{
    [Theory]
    [InlineData("")]
    [InlineData("2-8DAB-497B-94A3-B576943CA7AC")]
    public void ConvertToGuid_Returns_False_When_Providing_Invalid_Guids(string guid)
    {
        // Arrange
        var property = typeof(CarResultModel).GetProperty(nameof(CarResultModel.Id));
        var target = new CarResultModel();

        // Act
        var result = guid.ConvertToGuid(property!, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ConvertToGuid_Returns_True_When_Providing_Valid_Guid()
    {
        // Arrange
        var guid = "12345678-1234-1234-1234-123456789012";

        var property = typeof(CarResultModel).GetProperty(nameof(CarResultModel.Id));
        var target = new CarResultModel();

        // Act
        var result = guid.ConvertToGuid(property!, target);

        // Assert
        result.Should().BeTrue();
        target.Id.Should().Be(Guid.Parse(guid));
    }
}
