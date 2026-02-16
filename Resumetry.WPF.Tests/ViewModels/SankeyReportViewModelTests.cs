using FluentAssertions;
using Moq;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using System.Collections.Immutable;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

/// <summary>
/// Tests for SankeyReportViewModel's proper use of IScopedRunner and INavigationService.
/// </summary>
public class SankeyReportViewModelTests
{
    private readonly Mock<IScopedRunner> _mockScopedRunner;
    private readonly Mock<INavigationService> _mockNavigationService;

    public SankeyReportViewModelTests()
    {
        _mockScopedRunner = new Mock<IScopedRunner>();
        _mockNavigationService = new Mock<INavigationService>();

        // Default setup for RunAsync with ISankeyReportService - return empty list
        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ReturnsAsync(ImmutableList<SankeyReportData>.Empty);
    }

    [Fact]
    public void Constructor_WithValidDependencies_DoesNotThrow()
    {
        // Arrange & Act
        Action act = () => new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task LoadReportCommand_CallsScopedRunner()
    {
        // Arrange
        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        _mockScopedRunner.Verify(
            r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()),
            Times.Once());
    }

    [Fact]
    public async Task LoadReportCommand_SetsReportData()
    {
        // Arrange
        var testData = ImmutableList<SankeyReportData>.Empty.AddRange(
        [
            GetSankeyReportData("Applied", "No Response", 5),
            GetSankeyReportData("Applied", "Responded", 3)
        ]);

        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ReturnsAsync(testData);

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        viewModel.ReportData.Should().HaveCount(2);
        viewModel.ReportData.First().From.Should().Be("Applied");
        viewModel.ReportData.First().To.Should().Be("No Response");
        viewModel.ReportData.First().Count.Should().Be(5);
    }

    [Fact]
    public async Task LoadReportCommand_ComputesTotalApplications()
    {
        // Arrange
        var testData = ImmutableList<SankeyReportData>.Empty.AddRange(
        [
            GetSankeyReportData("Applied", "No Response", 5),
            GetSankeyReportData("Applied", "Responded", 3),
            GetSankeyReportData("Responded", "Rejected", 2),
            GetSankeyReportData("Responded", "Interview", 1)
        ]);

        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ReturnsAsync(testData);

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        // TotalApplications should be sum of counts where From == "Applied"
        viewModel.TotalApplications.Should().Be(8); // 5 + 3
    }

    [Fact]
    public async Task LoadReportCommand_WithNoData_SetsTotalApplicationsToZero()
    {
        // Arrange
        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ReturnsAsync(ImmutableList<SankeyReportData>.Empty);

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        viewModel.TotalApplications.Should().Be(0);
    }

    [Fact]
    public async Task LoadReportCommand_SetsIsLoadingToTrueDuringLoad()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource<ImmutableList<SankeyReportData>>();

        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .Returns(taskCompletionSource.Task);

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        var loadTask = viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert - should be loading
        viewModel.IsLoaded.Should().BeFalse();

        // Complete the task
        taskCompletionSource.SetResult(ImmutableList<SankeyReportData>.Empty);
        await loadTask;

        // Should no longer be loading
        viewModel.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task LoadReportCommand_OnError_SetsIsLoadingToFalse()
    {
        // Arrange
        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ThrowsAsync(new Exception("Test error"));

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task LoadReportCommand_OnError_DoesNotCrash()
    {
        // Arrange
        _mockScopedRunner
            .Setup(r => r.RunAsync<ISankeyReportService, ImmutableList<SankeyReportData>>(
                It.IsAny<Func<ISankeyReportService, Task<ImmutableList<SankeyReportData>>>>()))
            .ThrowsAsync(new Exception("Test error"));

        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        Func<Task> act = async () => await viewModel.LoadReportCommand.ExecuteAsync(null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void GoBackCommand_CallsNavigateToHome()
    {
        // Arrange
        var viewModel = new SankeyReportViewModel(_mockScopedRunner.Object, _mockNavigationService.Object);

        // Act
        viewModel.GoBackCommand.Execute(null);

        // Assert
        _mockNavigationService.Verify(
            n => n.NavigateToHome(),
            Times.Once());
    }

    private SankeyReportData GetSankeyReportData (string from, string to, int count)
    {
        var data = new SankeyReportData(from, to);
        for (int i = 0; i < count; i++)
        {
            data.Increment();
        }
        return data;
    }
}
