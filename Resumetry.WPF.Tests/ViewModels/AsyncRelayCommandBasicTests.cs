using FluentAssertions;
using Resumetry.ViewModels;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

public class AsyncRelayCommandBasicTests
{
    [Fact]
    public void Constructor_WithValidAction_CreatesCommand()
    {
        // Arrange & Act
        var command = new AsyncRelayCommand(async () => await Task.CompletedTask);

        // Assert
        command.Should().NotBeNull();
        command.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void CanExecute_WithNoCanExecuteFunc_ReturnsTrue()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () => await Task.CompletedTask);

        // Act & Assert
        command.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void CanExecute_WithCanExecuteFunc_ReturnsCorrectValue()
    {
        // Arrange
        var canExecute = true;
        var command = new AsyncRelayCommand(
            async () => await Task.CompletedTask,
            () => canExecute);

        // Act & Assert
        command.CanExecute(null).Should().BeTrue();

        canExecute = false;
        command.CanExecute(null).Should().BeFalse();
    }
}
