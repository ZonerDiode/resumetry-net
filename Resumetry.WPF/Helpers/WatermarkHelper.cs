using System.Windows;

namespace Resumetry.Helpers;

/// <summary>
/// Provides an attached property for displaying watermark/placeholder text in TextBox controls.
/// </summary>
public static class WatermarkHelper
{
    /// <summary>
    /// Identifies the Watermark attached property.
    /// </summary>
    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(string),
            typeof(WatermarkHelper),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets the watermark text for the specified TextBox.
    /// </summary>
    /// <param name="obj">The TextBox to get the watermark from.</param>
    /// <returns>The watermark text, or an empty string if not set.</returns>
    public static string GetWatermark(DependencyObject obj)
    {
        return (string?)obj.GetValue(WatermarkProperty) ?? string.Empty;
    }

    /// <summary>
    /// Sets the watermark text for the specified TextBox.
    /// </summary>
    /// <param name="obj">The TextBox to set the watermark on.</param>
    /// <param name="value">The watermark text to display.</param>
    public static void SetWatermark(DependencyObject obj, string value)
    {
        obj.SetValue(WatermarkProperty, value ?? string.Empty);
    }
}
