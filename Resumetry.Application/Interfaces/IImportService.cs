using Resumetry.Domain.Entities;

namespace Resumetry.Application.Interfaces
{
    public interface IImportService
    {
        Task<IEnumerable<JobApplication>> ImportFromJsonAsync(string filePath, CancellationToken cancellationToken = default);
        Task<IEnumerable<JobApplication>> ImportFromCsvAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
