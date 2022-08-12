using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.TableConfigurations
{
    public class MealPlanRecipeTableConfiguration : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.MealPlanId, t.RecipeId });
        }
    }
}
