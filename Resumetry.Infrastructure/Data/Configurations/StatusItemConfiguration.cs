using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;

namespace Resumetry.Infrastructure.Data.Configurations
{
    public class StatusItemConfiguration : IEntityTypeConfiguration<StatusItem>
    {
        public void Configure(EntityTypeBuilder<StatusItem> builder)
        {
            builder.HasKey(si => si.Id);

            builder.Property(si => si.Occurred)
                .IsRequired();

            builder.Property(si => si.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(si => si.CreatedAt)
                .IsRequired();

            builder.Property(si => si.UpdatedAt);

            // Shadow property for foreign key
            builder.Property<Guid>("JobApplicationId")
                .IsRequired();
        }
    }
}
