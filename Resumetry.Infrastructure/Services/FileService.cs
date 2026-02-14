using Resumetry.Application.Interfaces;

namespace Resumetry.Infrastructure.Services;

/// <summary>
/// Provides file system operations using System.IO.File.
/// </summary>
public class FileService : IFileService
{
    /// <inheritdoc />
    public Task<bool> FileExistsAsync(string path)
    {
        return Task.FromResult(File.Exists(path));
    }

    /// <inheritdoc />
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc />
    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
    {
        return File.WriteAllTextAsync(path, contents, cancellationToken);
    }
}
