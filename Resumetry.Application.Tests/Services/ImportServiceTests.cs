using FluentAssertions;
using Moq;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Interfaces;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for ImportService following TDD Red/Green/Refactor approach.
/// </summary>
public class ImportServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly ImportService _sut;

    public ImportServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockUnitOfWork.Setup(x => x.JobApplications).Returns(_mockRepo.Object);
        _sut = new ImportService(_mockFileService.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ImportFromJsonAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = "nonexistent.json";
        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(false);

        // Act & Assert
        await FluentActions.Invoking(() => _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken))
            .Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"File not found: {filePath}");
    }

    [Fact]
    public async Task ImportFromJsonAsync_ValidJsonSingleApplication_AddsEntityAndReturnsCount()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "Id": "11111111-1111-1111-1111-111111111111",
                "Company": "TechCorp",
                "Position": "Software Engineer",
                "Description": "Great opportunity",
                "Salary": "$100k",
                "TopJob": true,
                "SourcePage": "linkedin.com",
                "ReviewPage": "glassdoor.com",
                "LoginNotes": "Use SSO",
                "CreatedAt": "2024-01-15T10:00:00Z",
                "UpdatedAt": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.Id.Should().Be(new Guid("11111111-1111-1111-1111-111111111111"));
        capturedApp.Company.Should().Be("TechCorp");
        capturedApp.Position.Should().Be("Software Engineer");
        capturedApp.Description.Should().Be("Great opportunity");
        capturedApp.Salary.Should().Be("$100k");
        capturedApp.TopJob.Should().BeTrue();
        capturedApp.SourcePage.Should().Be("linkedin.com");
        capturedApp.ReviewPage.Should().Be("glassdoor.com");
        capturedApp.LoginNotes.Should().Be("Use SSO");
        capturedApp.CreatedAt.Should().Be(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        capturedApp.UpdatedAt.Should().Be(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_ValidJsonMultipleApplications_ReturnsCorrectCount()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "Id": "11111111-1111-1111-1111-111111111111",
                "Company": "TechCorp",
                "Position": "Software Engineer",
                "CreatedAt": "2024-01-15T10:00:00Z",
                "UpdatedAt": "2024-01-15T10:00:00Z"
            },
            {
                "Id": "22222222-2222-2222-2222-222222222222",
                "Company": "DataCo",
                "Position": "Data Scientist",
                "CreatedAt": "2024-01-16T10:00:00Z",
                "UpdatedAt": "2024-01-16T10:00:00Z"
            },
            {
                "Id": "33333333-3333-3333-3333-333333333333",
                "Company": "CloudCorp",
                "Position": "Cloud Engineer",
                "CreatedAt": "2024-01-17T10:00:00Z",
                "UpdatedAt": "2024-01-17T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        var capturedApps = new List<JobApplication>();
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApps.Add(app))
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(3);
        capturedApps.Should().Contain(a => a.Company == "TechCorp");
        capturedApps.Should().Contain(a => a.Company == "DataCo");
        capturedApps.Should().Contain(a => a.Company == "CloudCorp");
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromJsonAsync_NullDeserialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var filePath = "test.json";
        var json = "null";

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act & Assert
        await FluentActions.Invoking(() => _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to deserialize JSON content");
    }

    [Fact]
    public async Task ImportFromJsonAsync_EmptyArray_ReturnsZero()
    {
        // Arrange
        var filePath = "test.json";
        var json = "[]";

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(0);
        _mockRepo.Verify(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
