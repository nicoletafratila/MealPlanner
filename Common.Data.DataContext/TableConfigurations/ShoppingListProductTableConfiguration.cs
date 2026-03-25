using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class ShoppingListProductTableConfiguration
        : IEntityTypeConfiguration<ShoppingListProduct>
    {
        public void Configure(EntityTypeBuilder<ShoppingListProduct> builder)
        {
            builder.HasKey(t => new { t.ShoppingListId, t.ProductId });
        }
    }
}