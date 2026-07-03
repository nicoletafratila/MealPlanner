using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Data.TableConfigurations
{
    public class ShopDisplaySequenceTableConfiguration
        : IEntityTypeConfiguration<ShopDisplaySequence>
    {
        public void Configure(EntityTypeBuilder<ShopDisplaySequence> builder)
        {
            builder.HasKey(x => new { x.ShopId, x.ProductCategoryId });
        }
    }
}
