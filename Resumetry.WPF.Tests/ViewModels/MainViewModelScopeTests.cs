using CommunityToolkit.Mvvm.Input;
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
    public async Task LoadJobApplicationsCommand_LoadsData_UsingScopedRunner()
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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

        // Assert
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService, IEnumerable<JobApplicationSummaryDto>>(
                It.IsAny<Func<IJobApplicationService, Task<IEnumerable<JobApplicationSummaryDto>>>>()),
            Times.Once());
    }

    [Fact]
    public async Task LoadJobApplicationsCommand_CallsScopedRunner()
    {
        // Arrange
        var viewModel = new MainViewModel(_mockScopedRunner.Object, _mockDialogService.Object);

        // Act
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(
            d => d.ShowError(
                It.Is<string>(s => s.Contains("Test error")),
                It.IsAny<string>()),
            Times.Once());
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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        // Act
        await viewModel.DeleteApplicationCommand.ExecuteAsync(null);

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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        _mockScopedRunner.Invocations.Clear();

        // Act
        await viewModel.DeleteApplicationCommand.ExecuteAsync(null);

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
        await viewModel.LoadJobApplicationsCommand.ExecuteAsync(null);

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        _mockScopedRunner.Invocations.Clear();

        // Act
        await viewModel.DeleteApplicationCommand.ExecuteAsync(null);

        // Assert - should NOT call RunAsync for delete (only confirm was called)
        _mockScopedRunner.Verify(
            r => r.RunAsync<IJobApplicationService>(
                It.IsAny<Func<IJobApplicationService, Task>>()),
            Times.Never());
    }
}
