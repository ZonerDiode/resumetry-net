using Resumetry.Application.Interfaces;
using Resumetry.Infrastructure.Data.Repositories;

namespace Resumetry.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IJobApplicationRepository? _jobApplicationRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IJobApplicationRepository JobApplications
        {
            get
            {
                _jobApplicationRepository ??= new JobApplicationRepository(_context);
                return _jobApplicationRepository;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
