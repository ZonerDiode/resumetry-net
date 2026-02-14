namespace Resumetry.WPF.Services;

/// <summary>
/// Provides navigation functionality for the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a view associated with the specified ViewModel type.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type to navigate to.</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : class;

    /// <summary>
    /// Navigates to a view associated with the specified ViewModel type and configures the ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type to navigate to.</typeparam>
    /// <param name="configure">Action to configure the ViewModel before navigation.</param>
    void NavigateTo<TViewModel>(Action<TViewModel> configure) where TViewModel : class;

    /// <summary>
    /// Navigates to the home view (job application list).
    /// </summary>
    void NavigateToHome();
}
