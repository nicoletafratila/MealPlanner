using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.TableConfigurations
{
    public class MealPlanRecipeTableConfiguration : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.MealPlanId, t.RecipeId });
        }
    }
}
