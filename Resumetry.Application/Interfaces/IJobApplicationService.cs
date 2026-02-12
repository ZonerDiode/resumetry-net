using Resumetry.Application.DTOs;

namespace Resumetry.Application.Interfaces;

/// <summary>
/// Service for managing job application business logic.
/// </summary>
public interface IJobApplicationService
{
    /// <summary>
    /// Creates a new job application.
    /// </summary>
    /// <param name="dto">The job application data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the created job application.</returns>
    Task<Guid> CreateAsync(CreateJobApplicationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing job application.
    /// </summary>
    /// <param name="dto">The updated job application data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(UpdateJobApplicationDto dto, CancellationToken cancellationToken = default);
}
