using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
{
    public class JobTableConfiguration
        : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            builder.HasIndex(e => e.OwnerUserId);
            builder.HasIndex(e => e.Status);
        }
    }
}
