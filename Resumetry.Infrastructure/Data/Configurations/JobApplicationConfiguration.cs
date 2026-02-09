using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resumetry.Domain.Entities;

namespace Resumetry.Infrastructure.Data.Configurations
{
    public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
    {
        public void Configure(EntityTypeBuilder<JobApplication> builder)
        {
            builder.Property<Guid?>("Id")
                .ValueGeneratedOnAdd(); 

            builder.HasKey(ja => ja.Id);

            builder.Property(ja => ja.Company)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ja => ja.Position)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ja => ja.Description);

            builder.Property(ja => ja.Salary)
                .HasMaxLength(100);

            builder.Property(ja => ja.TopJob)
                .IsRequired();

            builder.Property(ja => ja.SourcePage)
                .HasMaxLength(500);

            builder.Property(ja => ja.ReviewPage)
                .HasMaxLength(500);

            builder.Property(ja => ja.LoginNotes);

            builder.Property(ja => ja.CreatedAt)
                .IsRequired();

            builder.Property(ja => ja.UpdatedAt);

            // Relationship with Recruiter (optional)
            builder.HasOne(ja => ja.Recruiter)
                .WithMany()
                .HasForeignKey("RecruiterId")
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship with ApplicationEvents (cascade delete)
            builder.HasMany(ja => ja.ApplicationEvents)
                .WithOne()
                .HasForeignKey("JobApplicationId")
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with StatusItems (cascade delete)
            builder.HasMany(ja => ja.StatusItems)
                .WithOne()
                .HasForeignKey("JobApplicationId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
