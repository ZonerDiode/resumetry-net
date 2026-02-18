using Resumetry.Domain.Entities;

namespace Resumetry.Application.Interfaces
{
    public interface IImportService
    {
        Task<int> ImportFromJsonAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
