using Resumetry.Application.Interfaces;
using Resumetry.Infrastructure.Data.Repositories;

namespace Resumetry.Infrastructure.Data
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        public IJobApplicationRepository JobApplications { get; } = new JobApplicationRepository(context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
