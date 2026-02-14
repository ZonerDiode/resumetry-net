using FluentAssertions;
using Moq;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

public class ShellViewModelTests
{
    [Fact]
    public void CurrentView_DefaultsToNull()
    {
        // Arrange
        var mockNavigationService = new Mock<INavigationService>();

        // Act
        var viewModel = new ShellViewModel(mockNavigationService.Object);

        // Assert
        viewModel.CurrentView.Should().BeNull();
    }

    [Fact]
    public void CurrentView_CanBeSet()
    {
        // Arrange
        var mockNavigationService = new Mock<INavigationService>();
        var viewModel = new ShellViewModel(mockNavigationService.Object);
        var testView = new object();

        // Act
        viewModel.CurrentView = testView;

        // Assert
        viewModel.CurrentView.Should().Be(testView);
    }

    [Fact]
    public void NavigateToHome_CallsNavigationService()
    {
        // Arrange
        var mockNavigationService = new Mock<INavigationService>();
        var viewModel = new ShellViewModel(mockNavigationService.Object);

        // Act
        viewModel.NavigateToHome();

        // Assert
        mockNavigationService.Verify(x => x.NavigateToHome(), Times.Once);
    }
}
