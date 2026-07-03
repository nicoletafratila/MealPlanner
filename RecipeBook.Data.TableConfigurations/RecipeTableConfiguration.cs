using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
{
    public class RecipeTableConfiguration
        : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
