using FluentAssertions;
using Moq;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using Resumetry.Domain.Interfaces;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for SankeyReportService following TDD Red/Green/Refactor approach.
/// </summary>
public class SankeyReportServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJobApplicationRepository> _mockRepository;
    private readonly SankeyReportService _sut;

    public SankeyReportServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IJobApplicationRepository>();
        _mockUnitOfWork.Setup(x => x.JobApplications).Returns(_mockRepository.Object);
        _sut = new SankeyReportService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithNoApplications_ReturnsAllCountsZero()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(6); // All 6 categories
        result.Should().OnlyContain(r => r.Count == 0);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithSingleAppliedStatus_IncrementsNoResponse()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Applied }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();
        var noResponse = result.FirstOrDefault(r => r.From == "Applied" && r.To == "No Response");
        noResponse.Should().NotBeNull();
        noResponse!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithRejectedStatus_IncrementsAppliedToRespondedAndRespondedToRejected()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Rejected }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();
        var responded = result.FirstOrDefault(r => r.From == "Applied" && r.To == "Responded");
        responded.Should().NotBeNull();
        responded!.Count.Should().Be(1);

        var rejected = result.FirstOrDefault(r => r.From == "Responded" && r.To == "Rejected");
        rejected.Should().NotBeNull();
        rejected!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithInterviewAndOffer_IncrementsFullPath()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-3), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Screen },
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-1), Status = StatusEnum.Interview },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Offer }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();

        var responded = result.FirstOrDefault(r => r.From == "Applied" && r.To == "Responded");
        responded.Should().NotBeNull();
        responded!.Count.Should().Be(1);

        var interview = result.FirstOrDefault(r => r.From == "Responded" && r.To == "Interview");
        interview.Should().NotBeNull();
        interview!.Count.Should().Be(1);

        var offer = result.FirstOrDefault(r => r.From == "Interview" && r.To == "Offer");
        offer.Should().NotBeNull();
        offer!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithInterviewButNoOffer_IncrementsNoOfferPath()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-3), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Screen },
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-1), Status = StatusEnum.Interview }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();

        var noOffer = result.FirstOrDefault(r => r.From == "Interview" && r.To == "No Offer");
        noOffer.Should().NotBeNull();
        noOffer!.Count.Should().Be(1);
    }

    [Fact]
    public async Task GenerateSankeyReport_ResultsSortedByCountDescending()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            // 3 with single status (No Response)
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company1",
                Position = "Engineer",
                ApplicationStatuses = [new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Applied }]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company2",
                Position = "Engineer",
                ApplicationStatuses = [new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Applied }]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company3",
                Position = "Engineer",
                ApplicationStatuses = [new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Applied }]
            },
            // 2 with rejection
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company4",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-1), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Rejected }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company5",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-1), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Rejected }
                ]
            },
            // 1 with offer
            new()
            {
                Id = Guid.NewGuid(),
                Company = "Company6",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-3), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Interview },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Offer }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);

        // Verify sorted by count descending
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].Count.Should().BeGreaterThanOrEqualTo(result[i + 1].Count,
                $"item at index {i} should have count >= item at index {i + 1}");
        }

        // The highest count should be Applied->No Response (3)
        result[0].From.Should().Be("Applied");
        result[0].To.Should().Be("No Response");
        result[0].Count.Should().Be(3);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithEmptyApplicationStatuses_SkipsApplication()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses = [] // Empty
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(r => r.Count == 0);
    }

    [Fact]
    public async Task GenerateSankeyReport_WithScreenStatus_CountsAsResponded()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                ApplicationStatuses =
                [
                    new ApplicationStatus { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Applied },
                    new ApplicationStatus { Occurred = DateTime.UtcNow, Status = StatusEnum.Screen }
                ]
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GenerateSankeyReport();

        // Assert
        result.Should().NotBeNull();

        var responded = result.FirstOrDefault(r => r.From == "Applied" && r.To == "Responded");
        responded.Should().NotBeNull();
        responded!.Count.Should().Be(1);

        var interview = result.FirstOrDefault(r => r.From == "Responded" && r.To == "Interview");
        interview.Should().NotBeNull();
        interview!.Count.Should().Be(1);

        var noOffer = result.FirstOrDefault(r => r.From == "Interview" && r.To == "No Offer");
        noOffer.Should().NotBeNull();
        noOffer!.Count.Should().Be(1);
    }
}
