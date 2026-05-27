using RecipeBook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class ProductCategoryTableConfiguration
        : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
