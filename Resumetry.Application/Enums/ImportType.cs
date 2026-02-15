namespace Resumetry.Application.Enums;

/// <summary>
/// Specifies the type of import service to use.
/// </summary>
public enum ImportType
{
    /// <summary>
    /// Standard JSON import using current format.
    /// </summary>
    Standard,

    /// <summary>
    /// Legacy JSON import for Angular/Python version compatibility.
    /// </summary>
    Legacy
}
