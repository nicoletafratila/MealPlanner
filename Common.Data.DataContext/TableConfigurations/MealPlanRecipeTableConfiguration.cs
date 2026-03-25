using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class MealPlanRecipeTableConfiguration
        : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> builder)
        {
            builder.HasKey(x => new { x.MealPlanId, x.RecipeId });
        }
    }
}