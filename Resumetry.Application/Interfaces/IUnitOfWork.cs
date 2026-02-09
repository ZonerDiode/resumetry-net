namespace Resumetry.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IJobApplicationRepository JobApplications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
