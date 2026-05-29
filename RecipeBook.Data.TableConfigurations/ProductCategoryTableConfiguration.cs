using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
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
