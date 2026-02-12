using Resumetry.Domain.Enums;

namespace Resumetry.Application.DTOs;

/// <summary>
/// Data transfer object for status item.
/// </summary>
public record StatusItemDto(
    DateTime Occurred,
    StatusEnum Status,
    Guid? Id = null);
