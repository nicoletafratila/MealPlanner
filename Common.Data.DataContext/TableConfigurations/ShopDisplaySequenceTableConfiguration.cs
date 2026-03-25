using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
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