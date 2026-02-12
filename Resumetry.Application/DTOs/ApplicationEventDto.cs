namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for application event.
/// </summary>
public record ApplicationEventDto(
    DateTime Occurred,
    string Description,
    Guid? Id = null);
