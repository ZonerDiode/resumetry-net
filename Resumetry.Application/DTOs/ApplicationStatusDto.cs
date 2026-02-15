using Resumetry.Domain.Enums;

namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for application status.
/// </summary>
public record ApplicationStatusDto(
    DateTime Occurred,
    StatusEnum Status,
    Guid? Id = null);
