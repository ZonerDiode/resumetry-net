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
    Task<Guid> CreateAsync(JobApplicationCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing job application.
    /// </summary>
    /// <param name="dto">The updated job application data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(JobApplicationUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all job applications as summary DTOs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of job application summaries.</returns>
    Task<IEnumerable<JobApplicationSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single job application by ID with full details.
    /// </summary>
    /// <param name="id">The job application ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The job application detail DTO, or null if not found.</returns>
    Task<JobApplicationDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a job application.
    /// </summary>
    /// <param name="id">The job application ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the job application is not found.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
