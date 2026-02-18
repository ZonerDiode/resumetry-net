using Microsoft.EntityFrameworkCore;
using Resumetry.Domain.Common;
using Resumetry.Domain.Entities;

namespace Resumetry.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<Recruiter> Recruiters => Set<Recruiter>();
        public DbSet<ApplicationEvent> ApplicationEvents => Set<ApplicationEvent>();
        public DbSet<ApplicationStatus> ApplicationStatuses => Set<ApplicationStatus>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
