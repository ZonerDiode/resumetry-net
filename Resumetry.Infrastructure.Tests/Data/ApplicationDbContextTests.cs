using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Resumetry.Domain.Entities;
using Resumetry.Infrastructure.Data;
using Xunit;

namespace Resumetry.Infrastructure.Tests.Data;

/// <summary>
/// Tests for ApplicationDbContext timestamp management.
/// </summary>
public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
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
        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

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

        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken); // Small delay to ensure different timestamp
        jobApplication.Company = "Updated Company";
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

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

        _context.JobApplications.Add(jobApplication);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken);
        jobApplication.Position = "Updated Position";
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        jobApplication.CreatedAt.Should().Be(default(DateTime));
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
        _context.JobApplications.Add(jobApplication);
        _context.SaveChanges();

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

        _context.JobApplications.Add(jobApplication);
        _context.SaveChanges();

        var originalCreatedAt = jobApplication.CreatedAt;

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken);
        jobApplication.Company = "Updated Company";
        _context.SaveChanges();

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
