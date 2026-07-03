using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.TableConfigurations
{
    public class ShopTableConfiguration
        : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
