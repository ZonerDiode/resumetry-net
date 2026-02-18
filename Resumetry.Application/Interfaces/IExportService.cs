using Resumetry.Domain.Entities;

namespace Resumetry.Application.Interfaces
{
    public interface IExportService
    {
        Task<int> ExportToJsonAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
