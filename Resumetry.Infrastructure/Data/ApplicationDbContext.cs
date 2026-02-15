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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
