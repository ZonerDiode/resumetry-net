using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resumetry.WPF.Services;
using Xunit;

namespace Resumetry.WPF.Tests.Services;

public class ScopedRunnerTests
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly ScopedRunner _sut;

    public ScopedRunnerTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

        _sut = new ScopedRunner(_mockScopeFactory.Object);
    }

    [Fact]
    public async Task RunAsync_WithVoidOperation_CreatesScope()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService>(async svc => await Task.CompletedTask);

        // Assert
        _mockScopeFactory.Verify(f => f.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithVoidOperation_ResolvesService()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService>(async svc => await Task.CompletedTask);

        // Assert
        _mockServiceProvider.Verify(p => p.GetService(typeof(ITestService)), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithVoidOperation_ExecutesOperation()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        var operationExecuted = false;

        // Act
        await _sut.RunAsync<ITestService>(async svc =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
        });

        // Assert
        operationExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_WithVoidOperation_DisposesScope()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService>(async svc => await Task.CompletedTask);

        // Assert
        _mockScope.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithResultOperation_CreatesScope()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService, string>(async svc => "result");

        // Assert
        _mockScopeFactory.Verify(f => f.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithResultOperation_ResolvesService()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService, string>(async svc => "result");

        // Assert
        _mockServiceProvider.Verify(p => p.GetService(typeof(ITestService)), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithResultOperation_ExecutesOperationAndReturnsResult()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        const string expectedResult = "test result";

        // Act
        var result = await _sut.RunAsync<ITestService, string>(async svc => expectedResult);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task RunAsync_WithResultOperation_DisposesScope()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        // Act
        await _sut.RunAsync<ITestService, string>(async svc => "result");

        // Assert
        _mockScope.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithResultOperation_PassesServiceToOperation()
    {
        // Arrange
        var mockService = new Mock<ITestService>();
        _mockServiceProvider
            .Setup(p => p.GetService(typeof(ITestService)))
            .Returns(mockService.Object);

        ITestService? receivedService = null;

        // Act
        await _sut.RunAsync<ITestService, int>(async svc =>
        {
            receivedService = svc;
            return 42;
        });

        // Assert
        receivedService.Should().BeSameAs(mockService.Object);
    }

    public interface ITestService { }
}
