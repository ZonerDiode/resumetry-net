namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new job application.
/// </summary>
public record JobApplicationCreateDto(
    string Company,
    string Position,
    string? Description = null,
    string? Salary = null,
    bool TopJob = false,
    string? SourcePage = null,
    string? ReviewPage = null,
    string? LoginNotes = null,
    RecruiterDto? Recruiter = null,
    List<StatusItemDto>? StatusItems = null,
    List<ApplicationEventDto>? ApplicationEvents = null);
