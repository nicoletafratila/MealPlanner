using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.TableConfigurations
{
    public class RecipeIngredientTableConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.RecipeId, t.IngredientId });
        }
    }
}
