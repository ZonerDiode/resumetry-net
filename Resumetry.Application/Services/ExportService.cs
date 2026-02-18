using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Resumetry.Application.Services;

/// <summary>
/// Service for exporting job application data to various formats.
/// </summary>
public class ExportService(IFileService fileService, IUnitOfWork unitOfWork) : IExportService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Exports job applications to JSON format.
    /// </summary>
    /// <param name="jobApplications">The collection of job applications to export.</param>
    /// <param name="filePath">The file path where the JSON will be written.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<int> ExportToJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var jobApplications = await unitOfWork.JobApplications.GetAllAsync(cancellationToken);

        var json = JsonSerializer.Serialize(jobApplications, _options);
        await fileService.WriteAllTextAsync(filePath, json, cancellationToken);

        return jobApplications.Count();
    }
}
