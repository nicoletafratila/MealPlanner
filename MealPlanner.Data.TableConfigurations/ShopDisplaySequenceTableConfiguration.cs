using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations
{
    public sealed class ShopDisplaySequenceTableConfiguration
        : IEntityTypeConfiguration<ShopDisplaySequence>
    {
        public void Configure(EntityTypeBuilder<ShopDisplaySequence> builder)
        {
            builder.HasKey(x => new { x.ShopId, x.ProductCategoryId });
        }
    }
}
