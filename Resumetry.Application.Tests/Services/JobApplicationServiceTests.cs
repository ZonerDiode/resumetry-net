using FluentAssertions;
using Moq;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using Resumetry.Domain.Interfaces;
using Xunit;

namespace Resumetry.Application.Tests.Services;

/// <summary>
/// Tests for JobApplicationService following TDD Red/Green/Refactor approach.
/// </summary>
public class JobApplicationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJobApplicationRepository> _mockRepository;
    private readonly JobApplicationService _sut;

    public JobApplicationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IJobApplicationRepository>();
        _mockUnitOfWork.Setup(x => x.JobApplications).Returns(_mockRepository.Object);
        _sut = new JobApplicationService(_mockUnitOfWork.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CallsAddAsyncAndSaveChanges()
    {
        // Arrange
        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Software Engineer");

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithRecruiter_SetsRecruiterOnEntity()
    {
        // Arrange
        var recruiterDto = new RecruiterDto(
            Name: "John Doe",
            Company: "RecruiterCo",
            Email: "john@recruiter.com",
            Phone: "555-1234");

        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Software Engineer",
            Recruiter: recruiterDto);

        JobApplication? capturedEntity = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((entity, _) => capturedEntity = entity);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.Recruiter.Should().NotBeNull();
        capturedEntity.Recruiter!.Name.Should().Be("John Doe");
        capturedEntity.Recruiter.Company.Should().Be("RecruiterCo");
        capturedEntity.Recruiter.Email.Should().Be("john@recruiter.com");
        capturedEntity.Recruiter.Phone.Should().Be("555-1234");
    }

    [Fact]
    public async Task CreateAsync_WithStatusItems_AddsStatusItemsToEntity()
    {
        // Arrange
        var statusItems = new List<StatusItemDto>
        {
            new(DateTime.UtcNow.AddDays(-2), StatusEnum.Applied),
            new(DateTime.UtcNow.AddDays(-1), StatusEnum.Screen)
        };

        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Software Engineer",
            StatusItems: statusItems);

        JobApplication? capturedEntity = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((entity, _) => capturedEntity = entity);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.StatusItems.Should().HaveCount(2);
        capturedEntity.StatusItems.Should().Contain(s => s.Status == StatusEnum.Applied);
        capturedEntity.StatusItems.Should().Contain(s => s.Status == StatusEnum.Screen);
    }

    [Fact]
    public async Task CreateAsync_WithApplicationEvents_AddsEventsToEntity()
    {
        // Arrange
        var events = new List<ApplicationEventDto>
        {
            new(DateTime.UtcNow.AddDays(-1), "Submitted application"),
            new(DateTime.UtcNow, "Received confirmation email")
        };

        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Software Engineer",
            ApplicationEvents: events);

        JobApplication? capturedEntity = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((entity, _) => capturedEntity = entity);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.ApplicationEvents.Should().HaveCount(2);
        capturedEntity.ApplicationEvents.Should().Contain(e => e.Description == "Submitted application");
        capturedEntity.ApplicationEvents.Should().Contain(e => e.Description == "Received confirmation email");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyCompany_ThrowsArgumentException()
    {
        // Arrange
        var dto = new JobApplicationCreateDto(
            Company: "",
            Position: "Software Engineer");

        // Act & Assert
        await FluentActions.Invoking(() => _sut.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Company*");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyPosition_ThrowsArgumentException()
    {
        // Arrange
        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "");

        // Act & Assert
        await FluentActions.Invoking(() => _sut.CreateAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Position*");
    }

    [Fact]
    public async Task CreateAsync_ReturnsNewGuid()
    {
        // Arrange
        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Software Engineer");

        JobApplication? capturedEntity = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((entity, _) => capturedEntity = entity);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.Id.Should().NotBeEmpty();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_UpdatesScalarProperties()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "OldCorp",
            Position = "Old Position",
            Description = "Old description",
            Salary = "$50k",
            TopJob = false,
            SourcePage = "old-source.com",
            ReviewPage = "old-review.com",
            LoginNotes = "Old notes"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "NewCorp",
            Position: "New Position",
            Description: "New description",
            Salary: "$100k",
            TopJob: true,
            SourcePage: "new-source.com",
            ReviewPage: "new-review.com",
            LoginNotes: "New notes");

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.Company.Should().Be("NewCorp");
        existingEntity.Position.Should().Be("New Position");
        existingEntity.Description.Should().Be("New description");
        existingEntity.Salary.Should().Be("$100k");
        existingEntity.TopJob.Should().BeTrue();
        existingEntity.SourcePage.Should().Be("new-source.com");
        existingEntity.ReviewPage.Should().Be("new-review.com");
        existingEntity.LoginNotes.Should().Be("New notes");
        _mockRepository.Verify(x => x.Update(existingEntity), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNewRecruiter_CreatesRecruiter()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            Recruiter = null
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var recruiterDto = new RecruiterDto(
            Name: "Jane Smith",
            Company: "RecruiterCo",
            Email: "jane@recruiter.com",
            Phone: "555-9999");

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            Recruiter: recruiterDto);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.Recruiter.Should().NotBeNull();
        existingEntity.Recruiter!.Name.Should().Be("Jane Smith");
        existingEntity.Recruiter.Company.Should().Be("RecruiterCo");
        existingEntity.Recruiter.Email.Should().Be("jane@recruiter.com");
        existingEntity.Recruiter.Phone.Should().Be("555-9999");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingRecruiter_UpdatesRecruiter()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            Recruiter = new Recruiter
            {
                Id = Guid.NewGuid(),
                Name = "Old Name",
                Company = "Old Company",
                Email = "old@email.com",
                Phone = "111-1111"
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var recruiterDto = new RecruiterDto(
            Name: "Updated Name",
            Company: "Updated Company",
            Email: "updated@email.com",
            Phone: "222-2222");

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            Recruiter: recruiterDto);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.Recruiter.Should().NotBeNull();
        existingEntity.Recruiter!.Name.Should().Be("Updated Name");
        existingEntity.Recruiter.Company.Should().Be("Updated Company");
        existingEntity.Recruiter.Email.Should().Be("updated@email.com");
        existingEntity.Recruiter.Phone.Should().Be("222-2222");
    }

    [Fact]
    public async Task UpdateAsync_WithNullRecruiter_RemovesRecruiter()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            Recruiter = new Recruiter
            {
                Id = Guid.NewGuid(),
                Name = "John Doe"
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            Recruiter: null);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.Recruiter.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithNewStatusItem_AddsToCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var statusItems = new List<StatusItemDto>
        {
            new(DateTime.UtcNow, StatusEnum.Applied, Id: null) // null Id means new
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            StatusItems: statusItems);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.StatusItems.Should().HaveCount(1);
        existingEntity.StatusItems.First().Status.Should().Be(StatusEnum.Applied);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingStatusItem_UpdatesInCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var statusItemId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            StatusItems = new List<StatusItem>
            {
                new StatusItem
                {
                    Id = statusItemId,
                    Occurred = DateTime.UtcNow.AddDays(-5),
                    Status = StatusEnum.Applied
                }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var newOccurred = DateTime.UtcNow.AddDays(-3);
        var statusItems = new List<StatusItemDto>
        {
            new(newOccurred, StatusEnum.Screen, Id: statusItemId) // existing Id
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            StatusItems: statusItems);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.StatusItems.Should().HaveCount(1);
        var updatedItem = existingEntity.StatusItems.First();
        updatedItem.Id.Should().Be(statusItemId);
        updatedItem.Status.Should().Be(StatusEnum.Screen);
        updatedItem.Occurred.Should().BeCloseTo(newOccurred, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WithRemovedStatusItem_RemovesFromCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var statusItemId1 = Guid.NewGuid();
        var statusItemId2 = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            StatusItems = new List<StatusItem>
            {
                new StatusItem { Id = statusItemId1, Occurred = DateTime.UtcNow, Status = StatusEnum.Applied },
                new StatusItem { Id = statusItemId2, Occurred = DateTime.UtcNow, Status = StatusEnum.Screen }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        // Only include one of the two existing items
        var statusItems = new List<StatusItemDto>
        {
            new(DateTime.UtcNow, StatusEnum.Applied, Id: statusItemId1)
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            StatusItems: statusItems);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.StatusItems.Should().HaveCount(1);
        existingEntity.StatusItems.First().Id.Should().Be(statusItemId1);
    }

    [Fact]
    public async Task UpdateAsync_WithNewEvent_AddsToCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var events = new List<ApplicationEventDto>
        {
            new(DateTime.UtcNow, "New event", Id: null) // null Id means new
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            ApplicationEvents: events);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.ApplicationEvents.Should().HaveCount(1);
        existingEntity.ApplicationEvents.First().Description.Should().Be("New event");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEvent_UpdatesInCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            ApplicationEvents = new List<ApplicationEvent>
            {
                new ApplicationEvent
                {
                    Id = eventId,
                    Occurred = DateTime.UtcNow.AddDays(-5),
                    Description = "Old description"
                }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var newOccurred = DateTime.UtcNow.AddDays(-3);
        var events = new List<ApplicationEventDto>
        {
            new(newOccurred, "Updated description", Id: eventId) // existing Id
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            ApplicationEvents: events);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.ApplicationEvents.Should().HaveCount(1);
        var updatedEvent = existingEntity.ApplicationEvents.First();
        updatedEvent.Id.Should().Be(eventId);
        updatedEvent.Description.Should().Be("Updated description");
        updatedEvent.Occurred.Should().BeCloseTo(newOccurred, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_WithRemovedEvent_RemovesFromCollection()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            ApplicationEvents = new List<ApplicationEvent>
            {
                new ApplicationEvent { Id = eventId1, Occurred = DateTime.UtcNow, Description = "Event 1" },
                new ApplicationEvent { Id = eventId2, Occurred = DateTime.UtcNow, Description = "Event 2" }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        // Only include one of the two existing events
        var events = new List<ApplicationEventDto>
        {
            new(DateTime.UtcNow, "Event 1", Id: eventId1)
        };

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            ApplicationEvents: events);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.ApplicationEvents.Should().HaveCount(1);
        existingEntity.ApplicationEvents.First().Id.Should().Be(eventId1);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication?)null);

        var dto = new JobApplicationUpdateDto(
            Id: nonExistentId,
            Company: "TechCorp",
            Position: "Engineer");

        // Act & Assert
        await FluentActions.Invoking(() => _sut.UpdateAsync(dto))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyCompany_ThrowsArgumentException()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "",
            Position: "Engineer");

        // Act & Assert
        await FluentActions.Invoking(() => _sut.UpdateAsync(dto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Company*");
    }

    [Fact]
    public async Task UpdateAsync_WithNullStatusItems_ClearsAllStatusItems()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            StatusItems = new List<StatusItem>
            {
                new StatusItem { Id = Guid.NewGuid(), Occurred = DateTime.UtcNow, Status = StatusEnum.Applied },
                new StatusItem { Id = Guid.NewGuid(), Occurred = DateTime.UtcNow, Status = StatusEnum.Screen }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            StatusItems: null);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.StatusItems.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WithNullApplicationEvents_ClearsAllEvents()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingEntity = new JobApplication
        {
            Id = existingId,
            Company = "TechCorp",
            Position = "Engineer",
            ApplicationEvents = new List<ApplicationEvent>
            {
                new ApplicationEvent { Id = Guid.NewGuid(), Occurred = DateTime.UtcNow, Description = "Event 1" },
                new ApplicationEvent { Id = Guid.NewGuid(), Occurred = DateTime.UtcNow, Description = "Event 2" }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var dto = new JobApplicationUpdateDto(
            Id: existingId,
            Company: "TechCorp",
            Position: "Engineer",
            ApplicationEvents: null);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        existingEntity.ApplicationEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithAllFields_MapsCorrectly()
    {
        // Arrange
        var recruiterDto = new RecruiterDto("John Doe", "RecruiterCo", "john@email.com", "555-1234");
        var statusItems = new List<StatusItemDto>
        {
            new(DateTime.UtcNow, StatusEnum.Applied)
        };
        var events = new List<ApplicationEventDto>
        {
            new(DateTime.UtcNow, "Applied online")
        };

        var dto = new JobApplicationCreateDto(
            Company: "TechCorp",
            Position: "Senior Engineer",
            Description: "Great opportunity",
            Salary: "$120k",
            TopJob: true,
            SourcePage: "linkedin.com",
            ReviewPage: "glassdoor.com",
            LoginNotes: "Use SSO",
            Recruiter: recruiterDto,
            StatusItems: statusItems,
            ApplicationEvents: events);

        JobApplication? capturedEntity = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()))
            .Callback<JobApplication, CancellationToken>((entity, _) => capturedEntity = entity);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        capturedEntity.Should().NotBeNull();
        capturedEntity!.Company.Should().Be("TechCorp");
        capturedEntity.Position.Should().Be("Senior Engineer");
        capturedEntity.Description.Should().Be("Great opportunity");
        capturedEntity.Salary.Should().Be("$120k");
        capturedEntity.TopJob.Should().BeTrue();
        capturedEntity.SourcePage.Should().Be("linkedin.com");
        capturedEntity.ReviewPage.Should().Be("glassdoor.com");
        capturedEntity.LoginNotes.Should().Be("Use SSO");
        capturedEntity.Recruiter.Should().NotBeNull();
        capturedEntity.StatusItems.Should().HaveCount(1);
        capturedEntity.ApplicationEvents.Should().HaveCount(1);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenNoApplicationsExist()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobApplication>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSummaryDtos_WithComputedFields()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var applications = new List<JobApplication>
        {
            new JobApplication
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                Salary = "$100k",
                TopJob = true,
                CreatedAt = createdAt
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.Id.Should().Be(applications[0].Id);
        dto.Company.Should().Be("TechCorp");
        dto.Position.Should().Be("Engineer");
        dto.Salary.Should().Be("$100k");
        dto.TopJob.Should().BeTrue();
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task GetAllAsync_ComputesCurrentStatus_FromLatestStatusItem()
    {
        // Arrange
        var applications = new List<JobApplication>
        {
            new JobApplication
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                StatusItems = new List<StatusItem>
                {
                    new StatusItem { Occurred = DateTime.UtcNow.AddDays(-5), Status = StatusEnum.Applied },
                    new StatusItem { Occurred = DateTime.UtcNow.AddDays(-2), Status = StatusEnum.Screen },
                    new StatusItem { Occurred = DateTime.UtcNow.AddDays(-1), Status = StatusEnum.Interview }
                }
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var dto = result.First();
        dto.CurrentStatus.Should().Be(StatusEnum.Interview);
        dto.CurrentStatusText.Should().Be("Interview");
    }

    [Fact]
    public async Task GetAllAsync_ComputesAppliedDate_FromFirstAppliedStatusItem()
    {
        // Arrange
        var appliedDate = DateTime.UtcNow.AddDays(-10);
        var applications = new List<JobApplication>
        {
            new JobApplication
            {
                Id = Guid.NewGuid(),
                Company = "TechCorp",
                Position = "Engineer",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                StatusItems = new List<StatusItem>
                {
                    new StatusItem { Occurred = appliedDate, Status = StatusEnum.Applied },
                    new StatusItem { Occurred = DateTime.UtcNow.AddDays(-5), Status = StatusEnum.Screen }
                }
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var dto = result.First();
        dto.AppliedDate.Should().BeCloseTo(appliedDate, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsDetailDto_WithAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var application = new JobApplication
        {
            Id = id,
            Company = "TechCorp",
            Position = "Engineer",
            Description = "Great job",
            Salary = "$100k",
            TopJob = true,
            SourcePage = "linkedin.com",
            ReviewPage = "glassdoor.com",
            LoginNotes = "Use SSO",
            CreatedAt = createdAt,
            StatusItems = new List<StatusItem>(),
            ApplicationEvents = new List<ApplicationEvent>()
        };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Company.Should().Be("TechCorp");
        result.Position.Should().Be("Engineer");
        result.Description.Should().Be("Great job");
        result.Salary.Should().Be("$100k");
        result.TopJob.Should().BeTrue();
        result.SourcePage.Should().Be("linkedin.com");
        result.ReviewPage.Should().Be("glassdoor.com");
        result.LoginNotes.Should().Be("Use SSO");
        result.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_MapsRecruiter_WhenPresent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var application = new JobApplication
        {
            Id = id,
            Company = "TechCorp",
            Position = "Engineer",
            Recruiter = new Recruiter
            {
                Name = "John Doe",
                Company = "RecruiterCo",
                Email = "john@email.com",
                Phone = "555-1234"
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Recruiter.Should().NotBeNull();
        result.Recruiter!.Name.Should().Be("John Doe");
        result.Recruiter.Company.Should().Be("RecruiterCo");
        result.Recruiter.Email.Should().Be("john@email.com");
        result.Recruiter.Phone.Should().Be("555-1234");
    }

    [Fact]
    public async Task GetByIdAsync_MapsStatusItemsAndEvents()
    {
        // Arrange
        var id = Guid.NewGuid();
        var statusItemId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var application = new JobApplication
        {
            Id = id,
            Company = "TechCorp",
            Position = "Engineer",
            StatusItems = new List<StatusItem>
            {
                new StatusItem { Id = statusItemId, Occurred = DateTime.UtcNow, Status = StatusEnum.Applied }
            },
            ApplicationEvents = new List<ApplicationEvent>
            {
                new ApplicationEvent { Id = eventId, Occurred = DateTime.UtcNow, Description = "Applied online" }
            }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.StatusItems.Should().HaveCount(1);
        result.StatusItems.First().Id.Should().Be(statusItemId);
        result.StatusItems.First().Status.Should().Be(StatusEnum.Applied);
        result.ApplicationEvents.Should().HaveCount(1);
        result.ApplicationEvents.First().Id.Should().Be(eventId);
        result.ApplicationEvents.First().Description.Should().Be("Applied online");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_CallsDeleteAndSaveChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var application = new JobApplication
        {
            Id = id,
            Company = "TechCorp",
            Position = "Engineer"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _mockRepository.Verify(x => x.Delete(application), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _sut.DeleteAsync(id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion
}
