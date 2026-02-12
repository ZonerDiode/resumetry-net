namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for recruiter information.
/// </summary>
public record RecruiterDto(
    string Name,
    string? Company = null,
    string? Email = null,
    string? Phone = null);
