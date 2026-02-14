using Microsoft.Extensions.DependencyInjection;
using Resumetry.ViewModels;
using System.Windows;

namespace Resumetry.WPF.Services;

/// <summary>
/// Implements navigation functionality for the application.
/// </summary>
public class NavigationService(IServiceScopeFactory scopeFactory) : INavigationService, IDisposable
{
    private readonly Dictionary<Type, Func<object, object>> _viewMappings = new();
    private IServiceScope? _currentScope;
    private ShellViewModel? _shellViewModel;

    /// <summary>
    /// Initializes the navigation service with the shell view model.
    /// Must be called before any navigation occurs.
    /// </summary>
    public void Initialize(ShellViewModel shellViewModel)
    {
        _shellViewModel = shellViewModel ?? throw new ArgumentNullException(nameof(shellViewModel));
    }

    /// <summary>
    /// Registers a view type to be associated with a ViewModel type.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
    /// <typeparam name="TView">The View type.</typeparam>
    public void RegisterView<TViewModel, TView>()
        where TViewModel : class
        where TView : FrameworkElement, new()
    {
        RegisterViewFactory<TViewModel>(viewModel =>
        {
            var view = new TView();
            view.DataContext = viewModel;

            // Store ViewModel reference for tests using reflection
            var viewModelProperty = typeof(TView).GetProperty("ViewModel");
            if (viewModelProperty != null && viewModelProperty.CanWrite)
            {
                viewModelProperty.SetValue(view, viewModel);
            }

            return view;
        });
    }

    /// <summary>
    /// Registers a factory function to create views for a ViewModel type.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
    /// <param name="factory">Factory function that creates a view given a view model.</param>
    protected void RegisterViewFactory<TViewModel>(Func<object, object> factory)
        where TViewModel : class
    {
        _viewMappings[typeof(TViewModel)] = factory;
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        NavigateTo<TViewModel>(null);
    }

    /// <inheritdoc/>
    public void NavigateTo<TViewModel>(Action<TViewModel>? configure) where TViewModel : class
    {
        // Dispose previous scope
        _currentScope?.Dispose();

        // Create new scope
        _currentScope = scopeFactory.CreateScope();

        // Resolve ViewModel
        var viewModel = _currentScope.ServiceProvider.GetRequiredService<TViewModel>();

        // Configure ViewModel if action provided
        configure?.Invoke(viewModel);

        // Create view
        if (!_viewMappings.TryGetValue(typeof(TViewModel), out var viewFactory))
        {
            throw new InvalidOperationException($"No view registered for ViewModel type {typeof(TViewModel).Name}");
        }

        var view = viewFactory(viewModel);

        // Set current view in shell
        if (_shellViewModel == null)
        {
            throw new InvalidOperationException("NavigationService has not been initialized. Call Initialize(ShellViewModel) first.");
        }

        _shellViewModel.CurrentView = view;
    }

    /// <inheritdoc/>
    public void NavigateToHome()
    {
        NavigateTo<JobApplicationListViewModel>();
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}
