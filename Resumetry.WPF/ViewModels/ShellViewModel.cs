using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels;

/// <summary>
/// ViewModel for the main application shell.
/// </summary>
public partial class ShellViewModel(INavigationService navigationService) : ViewModelBase
{
    /// <summary>
    /// Gets or sets the current view to display in the shell.
    /// </summary>
    [ObservableProperty]
    private object? _currentView;

    /// <summary>
    /// Navigates to the home view.
    /// </summary>
    [RelayCommand]
    public void NavigateToHome()
    {
        navigationService.NavigateToHome();
    }
}
