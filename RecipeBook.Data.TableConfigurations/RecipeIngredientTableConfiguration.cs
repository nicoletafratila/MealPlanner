using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations
{
    public class RecipeIngredientTableConfiguration
        : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.HasKey(x => new { x.RecipeId, x.ProductId });
            builder.HasIndex(x => x.ProductId);
            builder.HasOne(x => x.Unit)
                   .WithMany()
                   .HasForeignKey(x => x.UnitId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
