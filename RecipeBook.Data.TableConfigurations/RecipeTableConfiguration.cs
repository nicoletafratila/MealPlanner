using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
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
