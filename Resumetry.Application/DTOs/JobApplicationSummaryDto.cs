using Resumetry.Domain.Enums;

namespace Resumetry.Application.DTOs;

/// <summary>
/// Summary data transfer object for job application list views.
/// </summary>
public record JobApplicationSummaryDto(
    Guid Id,
    string Company,
    string Position,
    string? Salary,
    bool TopJob,
    DateTime CreatedAt,
    StatusEnum? CurrentStatus,
    string CurrentStatusText,
    DateTime? AppliedDate,
    RecruiterDto? Recruiter,
    List<ApplicationEventDto> ApplicationEvents);
