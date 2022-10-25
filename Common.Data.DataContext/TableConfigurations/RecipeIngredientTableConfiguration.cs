using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public class RecipeIngredientTableConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.RecipeId, t.IngredientId });
        }
    }
}
