using FluentAssertions;
using Moq;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for ImportService following TDD Red/Green/Refactor approach.
/// </summary>
public class ImportServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly ImportService _sut;

    public ImportServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _sut = new ImportService(_mockFileService.Object);
    }

    #region ImportFromJsonAsync Tests

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
    public async Task ImportFromJsonAsync_ValidJsonSingleApplication_ReturnsCorrectEntity()
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

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(1);
        var application = result.First();
        application.Id.Should().Be(new Guid("11111111-1111-1111-1111-111111111111"));
        application.Company.Should().Be("TechCorp");
        application.Position.Should().Be("Software Engineer");
        application.Description.Should().Be("Great opportunity");
        application.Salary.Should().Be("$100k");
        application.TopJob.Should().BeTrue();
        application.SourcePage.Should().Be("linkedin.com");
        application.ReviewPage.Should().Be("glassdoor.com");
        application.LoginNotes.Should().Be("Use SSO");
        application.CreatedAt.Should().Be(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc));
        application.UpdatedAt.Should().Be(new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc));
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

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Company == "TechCorp");
        result.Should().Contain(a => a.Company == "DataCo");
        result.Should().Contain(a => a.Company == "CloudCorp");
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
    public async Task ImportFromJsonAsync_EmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var filePath = "test.json";
        var json = "[]";

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
