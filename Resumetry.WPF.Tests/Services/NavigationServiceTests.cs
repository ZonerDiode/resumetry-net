using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using Xunit;

namespace Resumetry.WPF.Tests.Services;

public class NavigationServiceTests
{
    private class TestViewModel : ViewModelBase
    {
        public bool WasConfigured { get; set; }
    }

    private class TestView
    {
        public object? DataContext { get; set; }
        public TestViewModel? ViewModel { get; set; }
    }

    private class TestNavigationService : NavigationService
    {
        public TestNavigationService(IServiceScopeFactory scopeFactory)
            : base(scopeFactory)
        {
        }

        public new void RegisterView<TViewModel, TView>()
            where TViewModel : class
            where TView : class, new()
        {
            RegisterViewFactory<TViewModel>(viewModel =>
            {
                var view = new TView();
                // Use reflection to set DataContext if property exists
                var dataContextProp = typeof(TView).GetProperty("DataContext");
                dataContextProp?.SetValue(view, viewModel);

                // Use reflection to set ViewModel if property exists
                var viewModelProp = typeof(TView).GetProperty("ViewModel");
                viewModelProp?.SetValue(view, viewModel);

                return view;
            });
        }
    }

    [Fact]
    public void NavigateTo_SetsCurrentView()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestViewModel>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var shellViewModel = new ShellViewModel(Mock.Of<INavigationService>());
        var navigationService = new TestNavigationService(scopeFactory);
        navigationService.Initialize(shellViewModel);
        navigationService.RegisterView<TestViewModel, TestView>();

        // Act
        navigationService.NavigateTo<TestViewModel>();

        // Assert
        shellViewModel.CurrentView.Should().NotBeNull();
        shellViewModel.CurrentView.Should().BeOfType<TestView>();
    }

    [Fact]
    public void NavigateTo_ResolvesViewModel()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestViewModel>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var shellViewModel = new ShellViewModel(Mock.Of<INavigationService>());
        var navigationService = new TestNavigationService(scopeFactory);
        navigationService.Initialize(shellViewModel);
        navigationService.RegisterView<TestViewModel, TestView>();

        // Act
        navigationService.NavigateTo<TestViewModel>();

        // Assert
        var view = shellViewModel.CurrentView as TestView;
        view.Should().NotBeNull();
        view!.ViewModel.Should().NotBeNull();
        view.ViewModel.Should().BeOfType<TestViewModel>();
    }

    [Fact]
    public void NavigateTo_WithConfigure_InvokesConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestViewModel>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var shellViewModel = new ShellViewModel(Mock.Of<INavigationService>());
        var navigationService = new TestNavigationService(scopeFactory);
        navigationService.Initialize(shellViewModel);
        navigationService.RegisterView<TestViewModel, TestView>();

        // Act
        navigationService.NavigateTo<TestViewModel>(vm => vm.WasConfigured = true);

        // Assert
        var view = shellViewModel.CurrentView as TestView;
        view.Should().NotBeNull();
        view!.ViewModel.Should().NotBeNull();
        view.ViewModel!.WasConfigured.Should().BeTrue();
    }

    [Fact]
    public void NavigateTo_DisposessPreviousScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestViewModel>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var shellViewModel = new ShellViewModel(Mock.Of<INavigationService>());
        var navigationService = new TestNavigationService(scopeFactory);
        navigationService.Initialize(shellViewModel);
        navigationService.RegisterView<TestViewModel, TestView>();

        // Act
        navigationService.NavigateTo<TestViewModel>();
        var firstView = shellViewModel.CurrentView;

        navigationService.NavigateTo<TestViewModel>();
        var secondView = shellViewModel.CurrentView;

        // Assert
        firstView.Should().NotBeSameAs(secondView);
    }

    [Fact]
    public void Dispose_DisposesCurrentScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestViewModel>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var shellViewModel = new ShellViewModel(Mock.Of<INavigationService>());
        var navigationService = new TestNavigationService(scopeFactory);
        navigationService.Initialize(shellViewModel);
        navigationService.RegisterView<TestViewModel, TestView>();

        navigationService.NavigateTo<TestViewModel>();

        // Act & Assert (should not throw)
        navigationService.Dispose();
    }
}
