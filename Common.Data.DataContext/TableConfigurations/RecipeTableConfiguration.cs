using RecipeBook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class RecipeTableConfiguration
        : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
