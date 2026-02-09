using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Resumetry.Domain.Entities;

namespace Resumetry.Infrastructure.Data.Configurations
{
    public class ApplicationEventConfiguration : IEntityTypeConfiguration<ApplicationEvent>
    {
        public void Configure(EntityTypeBuilder<ApplicationEvent> builder)
        {
            builder.HasKey(ae => ae.Id);

            builder.Property(ae => ae.Date)
                .IsRequired();

            builder.Property(ae => ae.Description)
                .IsRequired();

            builder.Property(ae => ae.CreatedAt)
                .IsRequired();

            builder.Property(ae => ae.UpdatedAt);

            // Shadow property for foreign key
            builder.Property<Guid>("JobApplicationId")
                .IsRequired();
        }
    }
}
