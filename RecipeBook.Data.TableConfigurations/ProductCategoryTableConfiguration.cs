using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
{
    public class ProductCategoryTableConfiguration
        : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
