using FluentAssertions;
using Moq;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using System.Text.Json;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for ExportService following TDD Red/Green/Refactor approach.
/// </summary>
public class ExportServiceTests
{
    private readonly Mock<IFileService> _mockFileService;
    private readonly ExportService _sut;

    public ExportServiceTests()
    {
        _mockFileService = new Mock<IFileService>();
        _sut = new ExportService(_mockFileService.Object);
    }

    [Fact]
    public async Task ExportToJsonAsync_EmptyCollection_WritesEmptyJsonArray()
    {
        // Arrange
        var jobApplications = Enumerable.Empty<JobApplication>();
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        _mockFileService.Verify(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        capturedJson.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<List<object>>(capturedJson!);
        deserialized.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportToJsonAsync_SingleApplication_WritesValidJson()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Company = "TechCorp",
            Position = "Software Engineer",
            Description = "Great opportunity",
            Salary = "$100k",
            TopJob = true,
            SourcePage = "linkedin.com",
            ReviewPage = "glassdoor.com",
            LoginNotes = "Use SSO",
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        var array = document.RootElement;
        array.GetArrayLength().Should().Be(1);

        var app = array[0];
        app.GetProperty("Id").GetGuid().Should().Be(jobApplication.Id);
        app.GetProperty("Company").GetString().Should().Be("TechCorp");
        app.GetProperty("Position").GetString().Should().Be("Software Engineer");
        app.GetProperty("Description").GetString().Should().Be("Great opportunity");
        app.GetProperty("Salary").GetString().Should().Be("$100k");
        app.GetProperty("TopJob").GetBoolean().Should().BeTrue();
        app.GetProperty("SourcePage").GetString().Should().Be("linkedin.com");
        app.GetProperty("ReviewPage").GetString().Should().Be("glassdoor.com");
        app.GetProperty("LoginNotes").GetString().Should().Be("Use SSO");
    }

    [Fact]
    public async Task ExportToJsonAsync_WithRecruiter_IncludesRecruiterProperties()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Recruiter = new Recruiter
            {
                Name = "John Doe",
                Company = "RecruiterCo"
            }
        };

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        var app = document.RootElement[0];

        app.GetProperty("Recruiter").GetProperty("Name").GetString().Should().Be("John Doe");
        app.GetProperty("Recruiter").GetProperty("Company").GetString().Should().Be("RecruiterCo");
    }

    [Fact]
    public async Task ExportToJsonAsync_WithApplicationStatuses_IncludesStatusCollection()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        jobApplication.ApplicationStatuses.Add(new ApplicationStatus
        {
            Occurred = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Status = StatusEnum.Applied
        });

        jobApplication.ApplicationStatuses.Add(new ApplicationStatus
        {
            Occurred = new DateTime(2024, 1, 20, 10, 0, 0, DateTimeKind.Utc),
            Status = StatusEnum.Screen
        });

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        var app = document.RootElement[0];

        var statusItems = app.GetProperty("ApplicationStatuses");
        statusItems.GetArrayLength().Should().Be(2);
        statusItems[0].GetProperty("Status").GetString().Should().Be("Applied");
        statusItems[1].GetProperty("Status").GetString().Should().Be("Screen");
    }

    [Fact]
    public async Task ExportToJsonAsync_WithApplicationEvents_IncludesEventsCollection()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        jobApplication.ApplicationEvents.Add(new ApplicationEvent
        {
            Occurred = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Description = "Submitted application online"
        });

        jobApplication.ApplicationEvents.Add(new ApplicationEvent
        {
            Occurred = new DateTime(2024, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            Description = "Received confirmation email"
        });

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        var app = document.RootElement[0];

        var events = app.GetProperty("ApplicationEvents");
        events.GetArrayLength().Should().Be(2);
        events[0].GetProperty("Description").GetString().Should().Be("Submitted application online");
        events[1].GetProperty("Description").GetString().Should().Be("Received confirmation email");
    }

    [Fact]
    public async Task ExportToJsonAsync_WithNullOptionalFields_HandlesGracefully()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            Description = null,
            Salary = null,
            TopJob = false,
            SourcePage = null,
            ReviewPage = null,
            LoginNotes = null,
            Recruiter = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        var app = document.RootElement[0];

        app.GetProperty("Company").GetString().Should().Be("TechCorp");
        app.GetProperty("Position").GetString().Should().Be("Software Engineer");
        app.TryGetProperty("Description", out _).Should().BeTrue();
        app.TryGetProperty("Salary", out _).Should().BeTrue();
        app.TryGetProperty("Recruiter", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportToJsonAsync_UsesIndentedFormatting()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var jobApplications = new[] { jobApplication };
        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        capturedJson.Should().Contain("\n"); // Indented JSON should have newlines
        capturedJson.Should().Contain("  "); // Indented JSON should have spaces
    }

    [Fact]
    public async Task ExportToJsonAsync_MultipleApplications_WritesAllApplications()
    {
        // Arrange
        var jobApplications = new[]
        {
            new JobApplication
            {
                Company = "TechCorp",
                Position = "Software Engineer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new JobApplication
            {
                Company = "DataCo",
                Position = "Data Scientist",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new JobApplication
            {
                Company = "CloudCorp",
                Position = "Cloud Engineer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var filePath = "export.json";
        string? capturedJson = null;

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, content, _) => capturedJson = content)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, TestContext.Current.CancellationToken);

        // Assert
        capturedJson.Should().NotBeNull();
        var document = JsonDocument.Parse(capturedJson!);
        document.RootElement.GetArrayLength().Should().Be(3);

        document.RootElement[0].GetProperty("Company").GetString().Should().Be("TechCorp");
        document.RootElement[1].GetProperty("Company").GetString().Should().Be("DataCo");
        document.RootElement[2].GetProperty("Company").GetString().Should().Be("CloudCorp");
    }

    [Fact]
    public async Task ExportToJsonAsync_PassesCancellationToken()
    {
        // Arrange
        var jobApplications = new[] { new JobApplication
        {
            Company = "TechCorp",
            Position = "Software Engineer",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }};

        var filePath = "export.json";
        var cts = new CancellationTokenSource();

        _mockFileService.Setup(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExportToJsonAsync(jobApplications, filePath, cts.Token);

        // Assert
        _mockFileService.Verify(x => x.WriteAllTextAsync(
            filePath,
            It.IsAny<string>(),
            cts.Token),
            Times.Once);
    }
}
