using FluentAssertions;
using Moq;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for LegacyImportService following TDD Red/Green/Refactor approach.
/// </summary>
public class LegacyImportServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly LegacyImportService _sut;

    public LegacyImportServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _sut = new LegacyImportService(_mockFileService.Object);
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
        await FluentActions.Invoking(() => _sut.ImportFromJsonAsync(filePath))
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
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "description": "Great opportunity",
                "salary": "$100k",
                "topJob": true,
                "sourcePage": "linkedin.com",
                "reviewPage": "glassdoor.com",
                "loginHints": "Use SSO",
                "appliedDate": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

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
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z"
            },
            {
                "id": "22222222-2222-2222-2222-222222222222",
                "company": "DataCo",
                "role": "Data Scientist",
                "appliedDate": "2024-01-16T10:00:00Z"
            },
            {
                "id": "33333333-3333-3333-3333-333333333333",
                "company": "CloudCorp",
                "role": "Cloud Engineer",
                "appliedDate": "2024-01-17T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Company == "TechCorp");
        result.Should().Contain(a => a.Company == "DataCo");
        result.Should().Contain(a => a.Company == "CloudCorp");
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithRecruiterName_MapsRecruiterCorrectly()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "recruiterName": "John Doe",
                "recruiterCompany": "RecruiterCo",
                "appliedDate": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.Recruiter.Should().NotBeNull();
        application.Recruiter!.Name.Should().Be("John Doe");
        application.Recruiter.Company.Should().Be("RecruiterCo");
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithoutRecruiterName_DoesNotCreateRecruiter()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        result.First().Recruiter.Should().BeNull();
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithEmptyRecruiterName_DoesNotCreateRecruiter()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "recruiterName": "",
                "appliedDate": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        result.First().Recruiter.Should().BeNull();
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithApplicationStatuses_MapsCorrectly()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z",
                "status": [
                    {
                        "occurDate": "2024-01-15T10:00:00Z",
                        "status": "Applied"
                    },
                    {
                        "occurDate": "2024-01-20T10:00:00Z",
                        "status": "Screen"
                    },
                    {
                        "occurDate": "2024-01-25T10:00:00Z",
                        "status": "Interview"
                    }
                ]
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.ApplicationStatuses.Should().HaveCount(3);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Screen);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithInvalidStatus_SkipsInvalidApplicationtatus()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z",
                "status": [
                    {
                        "occurDate": "2024-01-15T10:00:00Z",
                        "status": "Applied"
                    },
                    {
                        "occurDate": "2024-01-20T10:00:00Z",
                        "status": "InvalidStatus"
                    },
                    {
                        "occurDate": "2024-01-25T10:00:00Z",
                        "status": "Interview"
                    }
                ]
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.ApplicationStatuses.Should().HaveCount(2);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
        application.ApplicationStatuses.Should().NotContain(s => s.Status.ToString() == "InvalidStatus");
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithNotes_MapsAsApplicationEvents()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z",
                "notes": [
                    {
                        "occurDate": "2024-01-15T10:00:00Z",
                        "description": "Submitted application online"
                    },
                    {
                        "occurDate": "2024-01-16T10:00:00Z",
                        "description": "Received confirmation email"
                    }
                ]
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.ApplicationEvents.Should().HaveCount(2);
        application.ApplicationEvents.Should().Contain(e => e.Description == "Submitted application online");
        application.ApplicationEvents.Should().Contain(e => e.Description == "Received confirmation email");
    }

    [Fact]
    public async Task ImportFromJsonAsync_WithEmptyNoteDescription_SkipsNote()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z",
                "notes": [
                    {
                        "occurDate": "2024-01-15T10:00:00Z",
                        "description": "Valid note"
                    },
                    {
                        "occurDate": "2024-01-16T10:00:00Z",
                        "description": ""
                    },
                    {
                        "occurDate": "2024-01-17T10:00:00Z",
                        "description": "Another valid note"
                    }
                ]
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.ApplicationEvents.Should().HaveCount(2);
        application.ApplicationEvents.Should().Contain(e => e.Description == "Valid note");
        application.ApplicationEvents.Should().Contain(e => e.Description == "Another valid note");
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
        await FluentActions.Invoking(() => _sut.ImportFromJsonAsync(filePath))
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
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportFromJsonAsync_NullOptionalFields_UsesDefaults()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "description": null,
                "salary": null,
                "topJob": false,
                "sourcePage": null,
                "reviewPage": null,
                "loginHints": null,
                "appliedDate": "2024-01-15T10:00:00Z"
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.Description.Should().Be(string.Empty);
        application.Salary.Should().Be(string.Empty);
        application.TopJob.Should().BeFalse();
        application.SourcePage.Should().BeNull();
        application.ReviewPage.Should().BeNull();
        application.LoginNotes.Should().BeNull();
    }

    [Fact]
    public async Task ImportFromJsonAsync_CaseInsensitiveStatusEnum_ParsesCorrectly()
    {
        // Arrange
        var filePath = "test.json";
        var json = """
        [
            {
                "id": "11111111-1111-1111-1111-111111111111",
                "company": "TechCorp",
                "role": "Software Engineer",
                "appliedDate": "2024-01-15T10:00:00Z",
                "status": [
                    {
                        "occurDate": "2024-01-15T10:00:00Z",
                        "status": "applied"
                    },
                    {
                        "occurDate": "2024-01-20T10:00:00Z",
                        "status": "SCREEN"
                    },
                    {
                        "occurDate": "2024-01-25T10:00:00Z",
                        "status": "InTeRvIeW"
                    }
                ]
            }
        ]
        """;

        _mockFileService.Setup(x => x.FileExistsAsync(filePath))
            .ReturnsAsync(true);
        _mockFileService.Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        // Act
        var result = await _sut.ImportFromJsonAsync(filePath);

        // Assert
        var application = result.First();
        application.ApplicationStatuses.Should().HaveCount(3);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Screen);
        application.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
    }

    #endregion

}
