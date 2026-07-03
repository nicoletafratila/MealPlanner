using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.TableConfigurations
{
    public class MealPlanRecipeTableConfiguration
        : IEntityTypeConfiguration<MealPlanRecipe>
    {
        public void Configure(EntityTypeBuilder<MealPlanRecipe> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasIndex(x => x.MealPlanId);
            builder.HasIndex(x => x.RecipeId);
        }
    }
}
