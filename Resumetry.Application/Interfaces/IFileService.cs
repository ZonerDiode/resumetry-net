namespace Resumetry.Application.Interfaces;

/// <summary>
/// Provides an abstraction over file system operations for testability.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    Task<bool> FileExistsAsync(string path);

    /// <summary>
    /// Asynchronously reads all text from the specified file.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The contents of the file as a string.</returns>
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously writes all text to the specified file.
    /// </summary>
    /// <param name="path">The file path to write.</param>
    /// <param name="contents">The text to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
}
