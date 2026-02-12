namespace Resumetry.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IJobApplicationRepository JobApplications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
