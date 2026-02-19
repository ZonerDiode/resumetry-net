using Resumetry.Domain.Enums;

namespace Resumetry.WPF.Services;

/// <summary>
/// Holds the result from the Add Status dialog.
/// </summary>
public record AddStatusResult(DateTime Occurred, StatusEnum Status);
