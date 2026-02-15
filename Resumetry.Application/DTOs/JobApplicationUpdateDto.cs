namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for updating an existing job application.
/// </summary>
public record JobApplicationUpdateDto(
    Guid Id,
    string Company,
    string Position,
    string? Description = null,
    string? Salary = null,
    bool TopJob = false,
    string? SourcePage = null,
    string? ReviewPage = null,
    string? LoginNotes = null,
    RecruiterDto? Recruiter = null,
    List<ApplicationStatusDto>? ApplicationStatuses = null,
    List<ApplicationEventDto>? ApplicationEvents = null);
