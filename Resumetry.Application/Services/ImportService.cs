using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Resumetry.Application.Services
{
    /// <summary>
    /// Provides functionality to import job applications from a JSON file using a specified file service.
    /// </summary>
    /// <param name="fileService">The service used to perform file operations, such as checking file existence and reading file content.</param>
    public class ImportService(IFileService fileService) : IImportService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<IEnumerable<JobApplication>> ImportFromJsonAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!await fileService.FileExistsAsync(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var jsonContent = await fileService.ReadAllTextAsync(filePath, cancellationToken);

            return JsonSerializer.Deserialize<List<JobApplication>>(jsonContent, _jsonOptions)
                   ?? throw new InvalidOperationException("Failed to deserialize JSON content");
        }
    }
}
