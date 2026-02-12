using Resumetry.Domain.Interfaces;
using Resumetry.Infrastructure.Data.Repositories;

namespace Resumetry.Infrastructure.Data
{
    /// <summary>
    /// Unit of Work implementation for managing repositories and committing changes to the database.
    /// Does not implement IDisposable as the ApplicationDbContext is expected to be managed by a dependency injection 
    /// container with appropriate lifetime management.
    /// </summary>
    /// <param name="context">The database context used for data operations.</param>
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public IJobApplicationRepository JobApplications { get; } = new JobApplicationRepository(context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
