using FluentAssertions;
using Moq;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Enums;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

/// <summary>
/// Tests for MainViewModel's proper use of IScopedRunner and IDialogService.
/// </summary>
public class MainViewModelScopeTests
{
    private readonly Mock<IScopedRunner> _mockScopedRunner;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly List<JobApplicationSummaryDto> _testSummaryDtos;

    public MainViewModelScopeTests()
    {
        _mockScopedRunner = new Mock<IScopedRunner>();
        _mockDialogService = new Mock<IDialogService>();
        _testSummaryDtos = [];

        // Default setup for RunAsync with IJobApplicationService - return empty list
        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(_testSummaryDtos);
    }

    [Fact]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        // Arrange & Act
        Action act = () => new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullScopedRunner_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new MainViewModel(null!, _mockDialogService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("scopedRunner");
    }

    [Fact]
    public void Constructor_WithNullDialogService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new MainViewModel(_mockScopedRunner.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dialogService");
    }

    [Fact]
    public async Task Constructor_LoadsInitialData_UsingScopedRunner()
    {
        // Arrange
        var summaryDtos = new List<JobApplicationSummaryDto>
        {
            new(
                Guid.NewGuid(),
                "Test Company",
                "Developer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now)
        };

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(summaryDtos);

        // Act
        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Assert
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task RefreshCommand_CallsScopedRunner()
    {
        // Arrange
        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        _mockScopedRunner.Invocations.Clear();

        // Act
        var refreshCommand = viewModel.RefreshCommand as AsyncRelayCommand;
        refreshCommand!.Execute(null);
        if (refreshCommand.RunningTask != null)
            await refreshCommand.RunningTask;

        // Assert
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()),
            Times.Once());
    }

    [Fact]
    public async Task LoadJobApplicationsAsync_PopulatesFilteredJobApplications()
    {
        // Arrange
        var summaryDtos = new List<JobApplicationSummaryDto>
        {
            new(
                Guid.NewGuid(),
                "Company A",
                "Developer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now),
            new(
                Guid.NewGuid(),
                "Company B",
                "Designer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now)
        };

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(summaryDtos);

        // Act
        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Assert
        viewModel.FilteredJobApplications.Should().HaveCount(2);
        viewModel.FilteredJobApplications.First().Company.Should().Be("Company A");
        viewModel.FilteredJobApplications.Last().Company.Should().Be("Company B");
    }

    [Fact]
    public async Task LoadJobApplicationsAsync_OnError_ShowsErrorDialog()
    {
        // Arrange
        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(150); // Give constructor's initial load time to complete and error to be handled

        // Assert
        _mockDialogService.Verify(
            d => d.ShowError(
                It.Is<string>(s => s.Contains("Test error")),
                It.IsAny<string>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task DeleteApplicationAsync_CallsConfirmDialog()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var summaryDtos = new List<JobApplicationSummaryDto>
        {
            new(
                testId,
                "Test Company",
                "Developer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now)
        };

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(summaryDtos);

        _mockDialogService
            .Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // User cancels

        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        // Act
        var deleteCommand = viewModel.DeleteApplicationCommand as AsyncRelayCommand;
        deleteCommand!.Execute(null);
        if (deleteCommand.RunningTask != null)
            await deleteCommand.RunningTask;

        // Assert
        _mockDialogService.Verify(
            d => d.Confirm(
                It.Is<string>(s => s.Contains("Test Company") && s.Contains("Developer")),
                It.Is<string>(s => s.Contains("Confirm Delete"))),
            Times.Once());
    }

    [Fact]
    public async Task DeleteApplicationAsync_WhenConfirmed_CallsScopedRunnerToDelete()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var summaryDtos = new List<JobApplicationSummaryDto>
        {
            new(
                testId,
                "Test Company",
                "Developer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now)
        };

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(summaryDtos);

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService>(
                It.IsAny<Func<IJobApplicationService, Task>>()))
            .Returns(Task.CompletedTask);

        _mockDialogService
            .Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // User confirms

        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        _mockScopedRunner.Invocations.Clear();

        // Act
        var deleteCommand = viewModel.DeleteApplicationCommand as AsyncRelayCommand;
        deleteCommand!.Execute(null);
        if (deleteCommand.RunningTask != null)
            await deleteCommand.RunningTask;

        // Assert - should call RunAsync for delete and then for refresh
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService>(
                It.IsAny<Func<IJobApplicationService, Task>>()),
            Times.Once());

        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()),
            Times.Once());
    }

    [Fact]
    public async Task DeleteApplicationAsync_WhenCancelled_DoesNotCallScopedRunner()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var summaryDtos = new List<JobApplicationSummaryDto>
        {
            new(
                testId,
                "Test Company",
                "Developer",
                null,
                false,
                DateTime.Now,
                StatusEnum.Applied,
                "Applied",
                DateTime.Now)
        };

        _mockScopedRunner
            .Setup(r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()))
            .ReturnsAsync(summaryDtos);

        _mockDialogService
            .Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // User cancels

        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        _mockScopedRunner.Invocations.Clear();

        // Act
        var deleteCommand = viewModel.DeleteApplicationCommand as AsyncRelayCommand;
        deleteCommand!.Execute(null);
        if (deleteCommand.RunningTask != null)
            await deleteCommand.RunningTask;

        // Assert - should NOT call RunAsync for delete (only confirm was called)
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService>(
                It.IsAny<Func<IJobApplicationService, Task>>()),
            Times.Never());
    }
}
