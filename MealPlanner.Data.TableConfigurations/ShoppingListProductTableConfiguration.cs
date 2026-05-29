using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations
{
    public sealed class ShoppingListProductTableConfiguration
        : IEntityTypeConfiguration<ShoppingListProduct>
    {
        public void Configure(EntityTypeBuilder<ShoppingListProduct> builder)
        {
            builder.HasKey(t => new { t.ShoppingListId, t.ProductId });
            builder.HasIndex(t => t.ProductId);
        }
    }
}
