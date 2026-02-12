using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Enums;
using Resumetry.ViewModels;
using Xunit;

namespace Resumetry.WPF.Tests.ViewModels;

/// <summary>
/// Tests for MainViewModel's proper use of IServiceScopeFactory to manage scoped service lifetimes.
/// </summary>
public class MainViewModelScopeTests
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockScopedServiceProvider;
    private readonly Mock<IJobApplicationService> _mockJobApplicationService;

    /// <summary>
    /// Testable subclass that bypasses UI dialogs.
    /// </summary>
    private class TestableMainViewModel : MainViewModel
    {
        public TestableMainViewModel(IServiceScopeFactory serviceScopeFactory)
            : base(serviceScopeFactory)
        {
        }

        protected override bool ConfirmDelete(JobApplicationViewModel app)
        {
            // Always confirm in tests - no dialog shown
            return true;
        }
    }

    public MainViewModelScopeTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopedServiceProvider = new Mock<IServiceProvider>();
        _mockJobApplicationService = new Mock<IJobApplicationService>();

        // Setup the scope factory to return the mock scope
        _mockScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(_mockScope.Object);

        // Setup the scope to return the scoped service provider
        _mockScope
            .Setup(s => s.ServiceProvider)
            .Returns(_mockScopedServiceProvider.Object);

        // Setup the scoped service provider to return the job application service
        _mockScopedServiceProvider
            .Setup(sp => sp.GetService(typeof(IJobApplicationService)))
            .Returns(_mockJobApplicationService.Object);

        // Default setup for GetAllAsync
        _mockJobApplicationService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<JobApplicationSummaryDto>());
    }

    [Fact]
    public void Constructor_WithServiceScopeFactory_DoesNotThrow()
    {
        // Arrange & Act
        Action act = () => new MainViewModel(_mockScopeFactory.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullServiceScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new MainViewModel(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceScopeFactory");
    }

    [Fact]
    public async Task LoadJobApplicationsAsync_CreatesScope()
    {
        // Arrange
        var viewModel = new MainViewModel(_mockScopeFactory.Object);

        // Act
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Assert - scope should have been created during initial load
        _mockScopeFactory.Verify(f => f.CreateScope(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task LoadJobApplicationsAsync_DisposesScope()
    {
        // Arrange
        var viewModel = new MainViewModel(_mockScopeFactory.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        _mockScope.Invocations.Clear();

        // Act
        var refreshCommand = viewModel.RefreshCommand as AsyncRelayCommand;
        refreshCommand!.Execute(null);
        if (refreshCommand.RunningTask != null)
            await refreshCommand.RunningTask;

        // Assert
        _mockScope.Verify(s => s.Dispose(), Times.Once());
    }

    [Fact]
    public async Task LoadJobApplicationsAsync_ResolvesJobApplicationService_FromScope()
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

        _mockJobApplicationService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(summaryDtos);

        var viewModel = new MainViewModel(_mockScopeFactory.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Act - verify service is resolved from the scoped service provider
        _mockScopedServiceProvider.Verify(
            sp => sp.GetService(typeof(IJobApplicationService)),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task DeleteApplicationAsync_CreatesAndDisposesScope()
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

        _mockJobApplicationService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(summaryDtos);

        _mockJobApplicationService
            .Setup(s => s.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Use testable subclass to bypass confirmation dialog
        var viewModel = new TestableMainViewModel(_mockScopeFactory.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Select the first application
        viewModel.SelectedJobApplication = viewModel.FilteredJobApplications.First();

        var scopeCreateCountBefore = _mockScopeFactory.Invocations.Count;
        var scopeDisposeCountBefore = _mockScope.Invocations.Count(i => i.Method.Name == "Dispose");

        // Act
        var deleteCommand = viewModel.DeleteApplicationCommand as AsyncRelayCommand;
        deleteCommand!.Execute(null);
        if (deleteCommand.RunningTask != null)
            await deleteCommand.RunningTask;

        // Assert - should have created at least one more scope (delete + refresh)
        _mockScopeFactory.Invocations.Count.Should().BeGreaterThan(scopeCreateCountBefore);

        // Assert - should have disposed the scopes
        var scopeDisposeCountAfter = _mockScope.Invocations.Count(i => i.Method.Name == "Dispose");
        scopeDisposeCountAfter.Should().BeGreaterThan(scopeDisposeCountBefore);
    }

    [Fact]
    public async Task RefreshCommand_CreatesNewScope_EachTime()
    {
        // Arrange
        var viewModel = new MainViewModel(_mockScopeFactory.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        var initialScopeCount = _mockScopeFactory.Invocations.Count;

        // Act - refresh twice
        var refreshCommand = viewModel.RefreshCommand as AsyncRelayCommand;
        refreshCommand!.Execute(null);
        if (refreshCommand.RunningTask != null)
            await refreshCommand.RunningTask;

        refreshCommand.Execute(null);
        if (refreshCommand.RunningTask != null)
            await refreshCommand.RunningTask;

        // Assert - should have created 2 more scopes (one per refresh)
        _mockScopeFactory.Invocations.Count.Should().Be(initialScopeCount + 2);
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

        _mockJobApplicationService
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(summaryDtos);

        // Act
        var viewModel = new MainViewModel(_mockScopeFactory.Object);
        await Task.Delay(100); // Give constructor's initial load time to complete

        // Assert
        viewModel.FilteredJobApplications.Should().HaveCount(2);
        viewModel.FilteredJobApplications.First().Company.Should().Be("Company A");
        viewModel.FilteredJobApplications.Last().Company.Should().Be("Company B");
    }
}
