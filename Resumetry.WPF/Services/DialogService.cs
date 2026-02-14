using System.Windows;

namespace Resumetry.WPF.Services;

/// <summary>
/// Provides message box functionality.
/// </summary>
public class DialogService : IDialogService
{
    /// <inheritdoc/>
    public bool Confirm(string message, string title)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return result == MessageBoxResult.Yes;
    }

    /// <inheritdoc/>
    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <inheritdoc/>
    public void ShowInfo(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
