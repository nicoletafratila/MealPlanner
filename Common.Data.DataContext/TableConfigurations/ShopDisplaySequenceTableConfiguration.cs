using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public class ShopDisplaySequenceTableConfiguration : IEntityTypeConfiguration<ShopDisplaySequence>
    {
        public void Configure(EntityTypeBuilder<ShopDisplaySequence> modelBuilder)
        {
            modelBuilder.HasKey(t => new { t.ShopId, t.ProductCategoryId });
        }
    }
}
