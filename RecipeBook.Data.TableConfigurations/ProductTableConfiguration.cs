using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
{
    public sealed class ProductTableConfiguration
        : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
