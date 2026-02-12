using FluentAssertions;
using Resumetry.ViewModels;
using System.Windows.Input;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

public class AsyncRelayCommandTests
{
    [Fact]
    public void Constructor_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new AsyncRelayCommand((Func<Task>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("execute");
    }

    [Fact]
    public async Task Execute_InvokesAsyncAction()
    {
        // Arrange
        var executed = false;
        var command = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Act
        command.Execute(null);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WithParameter_InvokesAsyncActionWithParameter()
    {
        // Arrange
        object? capturedParameter = null;
        var command = new AsyncRelayCommand(async (param) =>
        {
            await Task.Delay(10);
            capturedParameter = param;
        });

        // Act
        var testParam = new object();
        command.Execute(testParam);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert
        capturedParameter.Should().BeSameAs(testParam);
    }

    [Fact]
    public void CanExecute_WithNoCanExecuteFunc_ReturnsTrue()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () => await Task.CompletedTask);

        // Act
        var result = command.CanExecute(null);

        // Assert
        result.Should().BeTrue();
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

    [Fact]
    public async Task IsRunning_IsTrueDuringExecution()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(async () => await tcs.Task);

        // Act
        command.Execute(null);
        await Task.Yield(); // Give it time to start

        // Assert - should be running
        command.IsRunning.Should().BeTrue();

        // Complete the task
        tcs.SetResult(true);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert - should no longer be running
        command.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task CanExecute_IsFalseWhileRunning()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(async () => await tcs.Task);

        // Act
        command.CanExecute(null).Should().BeTrue();

        command.Execute(null);
        await Task.Yield(); // Give it time to start

        // Assert - should not be executable while running
        command.CanExecute(null).Should().BeFalse();

        // Complete the task
        tcs.SetResult(true);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert - should be executable again
        command.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task Execute_HandlesExceptionViaErrorHandler()
    {
        // Arrange
        Exception? capturedException = null;
        var expectedException = new InvalidOperationException("Test error");
        var command = new AsyncRelayCommand(
            async () =>
            {
                await Task.Delay(10);
                throw expectedException;
            },
            ex => capturedException = ex);

        // Act
        command.Execute(null);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert
        capturedException.Should().BeSameAs(expectedException);
        command.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WithoutErrorHandler_DoesNotThrow()
    {
        // Arrange
        var command = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test error");
        });

        // Act - should not throw even though the async operation throws
        Action act = () => command.Execute(null);

        // Assert
        act.Should().NotThrow();
        if (command.RunningTask != null)
            await command.RunningTask;
        command.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_PreventsReentrantExecution()
    {
        // Arrange
        var executionCount = 0;
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(async () =>
        {
            executionCount++;
            await tcs.Task;
        });

        // Act - try to execute twice
        command.Execute(null);
        await Task.Yield(); // Give first execution time to start

        command.Execute(null); // This should be ignored
        await Task.Yield();

        // Assert - should only execute once
        executionCount.Should().Be(1);

        // Complete the task
        tcs.SetResult(true);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Now should be able to execute again
        command.Execute(null);
        if (command.RunningTask != null)
            await command.RunningTask;
        executionCount.Should().Be(2);
    }

    [Fact]
    public async Task PropertyChanged_RaisedForIsRunning()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(async () => await tcs.Task);
        var propertyChangedEvents = new List<string>();

        command.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != null)
                propertyChangedEvents.Add(e.PropertyName);
        };

        // Act
        command.Execute(null);
        await Task.Yield();

        tcs.SetResult(true);
        if (command.RunningTask != null)
            await command.RunningTask;

        // Assert
        propertyChangedEvents.Should().Contain(nameof(AsyncRelayCommand.IsRunning));
        propertyChangedEvents.Count.Should().BeGreaterThanOrEqualTo(2); // At least once for start and once for finish
    }
}
