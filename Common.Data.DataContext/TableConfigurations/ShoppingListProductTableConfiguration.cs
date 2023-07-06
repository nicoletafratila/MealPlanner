using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public class ShoppingListProductTableConfiguration : IEntityTypeConfiguration<ShoppingListProduct>
    {
        public void Configure(EntityTypeBuilder<ShoppingListProduct> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.ShoppingListId, t.ProductId });
        }
    }
}
