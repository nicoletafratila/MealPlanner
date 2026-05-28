using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.TableConfigurations
{
    public sealed class MealPlanRecipeTableConfiguration
        : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> builder)
        {
            builder.HasKey(x => new { x.MealPlanId, x.RecipeId });
            builder.HasIndex(x => x.RecipeId);
        }
    }
}
