using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class RecipeIngredientTableConfiguration
        : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.HasKey(x => new { x.RecipeId, x.ProductId });
        }
    }
}