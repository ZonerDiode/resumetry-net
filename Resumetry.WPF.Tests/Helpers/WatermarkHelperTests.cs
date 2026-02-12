using System.Windows;
using System.Windows.Controls;
using FluentAssertions;
using Resumetry.Helpers;
using Xunit;

namespace Resumetry.WPF.Tests.Helpers;

/// <summary>
/// Unit tests for WatermarkHelper attached property.
/// </summary>
public class WatermarkHelperTests
{
    [StaFact]
    public void Watermark_DefaultValue_ShouldBeEmptyString()
    {
        // Arrange
        var textBox = new TextBox();

        // Act
        var watermark = WatermarkHelper.GetWatermark(textBox);

        // Assert
        watermark.Should().Be(string.Empty);
    }

    [StaFact]
    public void Watermark_SetAndGet_ShouldRoundTrip()
    {
        // Arrange
        var textBox = new TextBox();
        const string expectedWatermark = "Enter text here...";

        // Act
        WatermarkHelper.SetWatermark(textBox, expectedWatermark);
        var actualWatermark = WatermarkHelper.GetWatermark(textBox);

        // Assert
        actualWatermark.Should().Be(expectedWatermark);
    }

    [StaFact]
    public void Watermark_Property_ShouldHaveCorrectMetadata()
    {
        // Arrange & Act
        var property = WatermarkHelper.WatermarkProperty;

        // Assert
        property.Should().NotBeNull();
        property.Name.Should().Be("Watermark");
        property.PropertyType.Should().Be(typeof(string));
        property.OwnerType.Should().Be(typeof(WatermarkHelper));
    }

    [StaFact]
    public void Watermark_SetNull_ShouldReturnEmptyString()
    {
        // Arrange
        var textBox = new TextBox();

        // Act
        WatermarkHelper.SetWatermark(textBox, null!);
        var actualWatermark = WatermarkHelper.GetWatermark(textBox);

        // Assert
        actualWatermark.Should().Be(string.Empty);
    }

    [StaFact]
    public void Watermark_SetOnMultipleControls_ShouldMaintainSeparateValues()
    {
        // Arrange
        var textBox1 = new TextBox();
        var textBox2 = new TextBox();
        const string watermark1 = "First watermark";
        const string watermark2 = "Second watermark";

        // Act
        WatermarkHelper.SetWatermark(textBox1, watermark1);
        WatermarkHelper.SetWatermark(textBox2, watermark2);

        // Assert
        WatermarkHelper.GetWatermark(textBox1).Should().Be(watermark1);
        WatermarkHelper.GetWatermark(textBox2).Should().Be(watermark2);
    }
}
