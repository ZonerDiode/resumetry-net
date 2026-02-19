using System.Windows;
using Resumetry.Domain.Enums;
using Resumetry.ViewModels;
using Resumetry.Views;

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

    /// <inheritdoc/>
    public AddStatusResult? ShowAddStatusDialog(IEnumerable<StatusEnum> currentStatuses)
    {
        var vm = new AddStatusDialogViewModel(currentStatuses);
        var dialog = new AddStatusDialog(vm)
        {
            Owner = System.Windows.Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = System.Windows.Application.Current.MainWindow.Left + 300,
            Top = System.Windows.Application.Current.MainWindow.Top + 300
        };

        return dialog.ShowDialog() == true
            ? new AddStatusResult(vm.Occurred, vm.SelectedStatus)
            : null;
    }
}
