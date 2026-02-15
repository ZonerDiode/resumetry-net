using Resumetry.Domain.Entities;

namespace Resumetry.Domain.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default);
        void Update(JobApplication jobApplication);
        void Delete(JobApplication jobApplication);
    }
}
