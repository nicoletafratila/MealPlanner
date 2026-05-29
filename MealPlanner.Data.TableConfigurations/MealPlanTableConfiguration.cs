using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations
{
    public sealed class MealPlanTableConfiguration
        : IEntityTypeConfiguration<MealPlan>
    {
        public void Configure(EntityTypeBuilder<MealPlan> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
