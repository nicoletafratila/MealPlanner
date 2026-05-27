using RecipeBook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Common.Data.DataContext.TableConfigurations
{
    public sealed class RecipeCategoryTableConfiguration
        : IEntityTypeConfiguration<RecipeCategory>
    {
        public void Configure(EntityTypeBuilder<RecipeCategory> builder)
        {
            builder.HasIndex(e => e.UserId);
        }
    }
}
