using Microsoft.EntityFrameworkCore;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Interfaces;

namespace Resumetry.Infrastructure.Data.Repositories
{
    public class JobApplicationRepository(ApplicationDbContext context) : IJobApplicationRepository
    {
        public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default)
        {
            await context.JobApplications.AddAsync(jobApplication, cancellationToken);
        }

        public void Update(JobApplication jobApplication)
        {
            context.JobApplications.Update(jobApplication);
        }

        public void Delete(JobApplication jobApplication)
        {
            context.JobApplications.Remove(jobApplication);
        }
    }
}
