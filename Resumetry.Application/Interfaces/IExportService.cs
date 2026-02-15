using Resumetry.Domain.Entities;

namespace Resumetry.Application.Interfaces
{
    public interface IExportService
    {
        Task ExportToJsonAsync(IEnumerable<JobApplication> jobApplications, string filePath, CancellationToken cancellationToken = default);
    }
}
