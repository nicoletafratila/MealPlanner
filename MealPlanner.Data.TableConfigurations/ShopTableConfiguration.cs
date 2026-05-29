using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations
{
    public sealed class ShopTableConfiguration
        : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
