using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resumetry.Domain.Entities;

namespace Resumetry.Infrastructure.Data.Configurations
{
    public class RecruiterConfiguration : IEntityTypeConfiguration<Recruiter>
    {
        public void Configure(EntityTypeBuilder<Recruiter> builder)
        {
            builder.Property<Guid?>("Id")
                .ValueGeneratedOnAdd(); 

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Company)
                .HasMaxLength(200);

            builder.Property(r => r.Email)
                .HasMaxLength(200);

            builder.Property(r => r.Phone)
                .HasMaxLength(50);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt);
        }
    }
}
