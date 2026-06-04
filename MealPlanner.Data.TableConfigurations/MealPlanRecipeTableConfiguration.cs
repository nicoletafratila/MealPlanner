using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations
{
    public class MealPlanRecipeTableConfiguration
        : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> builder)
        {
            builder.HasKey(x => new { x.MealPlanId, x.RecipeId });
            builder.HasIndex(x => x.RecipeId);
        }
    }
}
