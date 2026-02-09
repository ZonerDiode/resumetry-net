using Microsoft.EntityFrameworkCore;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;

namespace Resumetry.Infrastructure.Data.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .FirstOrDefaultAsync(ja => ja.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<JobApplication>> GetTopJobsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .Where(ja => ja.TopJob)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<JobApplication>> GetByCompanyAsync(string company, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.Recruiter)
                .Include(ja => ja.ApplicationEvents)
                .Include(ja => ja.StatusItems)
                .Where(ja => ja.Company.Contains(company))
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default)
        {
            await _context.JobApplications.AddAsync(jobApplication, cancellationToken);
        }

        public void Update(JobApplication jobApplication)
        {
            _context.JobApplications.Update(jobApplication);
        }

        public void Delete(JobApplication jobApplication)
        {
            _context.JobApplications.Remove(jobApplication);
        }
    }
}
