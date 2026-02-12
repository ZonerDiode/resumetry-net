using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Resumetry.Views;
using Resumetry.ViewModels;

namespace Resumetry.WPF.Services;

/// <summary>
/// Provides dialog and message box functionality using WPF windows.
/// </summary>
public class DialogService(IServiceScopeFactory serviceScopeFactory) : IDialogService
{
    /// <inheritdoc/>
    public bool ShowApplicationForm()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var formViewModel = scope.ServiceProvider.GetRequiredService<ApplicationFormViewModel>();
        var formWindow = new ApplicationFormWindow(formViewModel);
        return formWindow.ShowDialog() == true;
    }

    /// <inheritdoc/>
    public bool ShowApplicationForm(Action<ApplicationFormViewModel> configure)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var formViewModel = scope.ServiceProvider.GetRequiredService<ApplicationFormViewModel>();
        configure(formViewModel);
        var formWindow = new ApplicationFormWindow(formViewModel);
        return formWindow.ShowDialog() == true;
    }

    /// <inheritdoc/>
    public void ShowSettings()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var settingsViewModel = scope.ServiceProvider.GetRequiredService<SettingsViewModel>();
        var settingsWindow = new SettingsWindow(settingsViewModel);
        settingsWindow.ShowDialog();
    }

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
