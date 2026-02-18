using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Resumetry.Domain.Entities;
using Resumetry.Infrastructure.Data;
using Xunit;

namespace Resumetry.Infrastructure.Tests.Data;

/// <summary>
/// Tests for ApplicationDbContext timestamp management.
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_AddedEntity_SetsCreatedAt()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        // CreatedAt should be default before save
        jobApplication.CreatedAt.Should().Be(default(DateTime));

        // Act
        await _unitOfWork.JobApplications.AddAsync(jobApplication, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        jobApplication.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        jobApplication.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_SetsUpdatedAt()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        await _unitOfWork.JobApplications.AddAsync(jobApplication, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken); // Small delay to ensure different timestamp
        jobApplication.Company = "Updated Company";
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        jobApplication.UpdatedAt.Should().NotBeNull();
        jobApplication.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        jobApplication.CreatedAt.Should().Be(originalCreatedAt); // CreatedAt should not change
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedEntity_DoesNotOverwriteCreatedAt()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        await _unitOfWork.JobApplications.AddAsync(jobApplication, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken);
        jobApplication.Position = "Updated Position";
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        jobApplication.CreatedAt.Should().Be(originalCreatedAt);
        jobApplication.UpdatedAt.Should().NotBeNull();
        jobApplication.UpdatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Fact]
    public void NewEntity_CreatedAt_IsDefault()
    {
        // Arrange & Act
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        // Assert
        jobApplication.CreatedAt.Should().Be(default);
    }

    [Fact]
    public async Task SaveChanges_AddedEntity_SetsCreatedAt()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        // Act
        await _unitOfWork.JobApplications.AddAsync(jobApplication, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        jobApplication.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        jobApplication.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_SetsUpdatedAt()
    {
        // Arrange
        var jobApplication = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = "Test Company",
            Position = "Test Position"
        };

        await _unitOfWork.JobApplications.AddAsync(jobApplication, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken);
        jobApplication.Company = "Updated Company";
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        jobApplication.UpdatedAt.Should().NotBeNull();
        jobApplication.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        jobApplication.CreatedAt.Should().Be(originalCreatedAt);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
