namespace Resumetry.Application.DTOs;

/// <summary>
/// Detailed data transfer object for job application edit/detail views.
/// </summary>
public record JobApplicationDetailDto(
    Guid Id,
    string Company,
    string Position,
    string? Description,
    string? Salary,
    bool TopJob,
    string? SourcePage,
    string? ReviewPage,
    string? LoginNotes,
    DateTime CreatedAt,
    RecruiterDto? Recruiter,
    List<StatusItemDto> StatusItems,
    List<ApplicationEventDto> ApplicationEvents);
