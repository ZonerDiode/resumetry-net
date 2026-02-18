using FluentAssertions;
using Moq;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using Resumetry.Domain.Interfaces;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for LegacyImportService following TDD Red/Green/Refactor approach.
/// </summary>
public class LegacyImportServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly LegacyImportService _sut;

    public LegacyImportServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IJobApplicationRepository>();
        _mockUnitOfWork.Setup(x => x.JobApplications).Returns(_mockRepo.Object);
        _sut = new LegacyImportService(_mockFileService.Object, _mockUnitOfWork.Object);
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.Recruiter.Should().NotBeNull();
        capturedApp.Recruiter!.Name.Should().Be("John Doe");
        capturedApp.Recruiter.Company.Should().Be("RecruiterCo");
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedApp.Should().NotBeNull();
        capturedApp!.Recruiter.Should().BeNull();
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedApp.Should().NotBeNull();
        capturedApp!.Recruiter.Should().BeNull();
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.ApplicationStatuses.Should().HaveCount(3);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Screen);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.ApplicationStatuses.Should().HaveCount(2);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
        capturedApp.ApplicationStatuses.Should().NotContain(s => s.Status.ToString() == "InvalidStatus");
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.ApplicationEvents.Should().HaveCount(2);
        capturedApp.ApplicationEvents.Should().Contain(e => e.Description == "Submitted application online");
        capturedApp.ApplicationEvents.Should().Contain(e => e.Description == "Received confirmation email");
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.ApplicationEvents.Should().HaveCount(2);
        capturedApp.ApplicationEvents.Should().Contain(e => e.Description == "Valid note");
        capturedApp.ApplicationEvents.Should().Contain(e => e.Description == "Another valid note");
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.Description.Should().Be(string.Empty);
        capturedApp.Salary.Should().Be(string.Empty);
        capturedApp.TopJob.Should().BeFalse();
        capturedApp.SourcePage.Should().BeNull();
        capturedApp.ReviewPage.Should().BeNull();
        capturedApp.LoginNotes.Should().BeNull();
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

        JobApplication? capturedApp = null;
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((app, _) => capturedApp = app)
            .Returns(Task.CompletedTask);

        // Act
        var count = await _sut.ImportFromJsonAsync(filePath, TestContext.Current.CancellationToken);

        // Assert
        count.Should().Be(1);
        capturedApp.Should().NotBeNull();
        capturedApp!.ApplicationStatuses.Should().HaveCount(3);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Applied);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Screen);
        capturedApp.ApplicationStatuses.Should().Contain(s => s.Status == StatusEnum.Interview);
    }
}
