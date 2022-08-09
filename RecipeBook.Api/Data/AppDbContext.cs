using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //seed units of measurements
            //modelBuilder.Entity<Unit>().HasData(new Country { CountryId = 1, Name = "Belgium" });

            modelBuilder.Entity<RecipeIngredient>()
             .HasKey(t => new { t.RecipeId, t.IngredientId });
        }
    }
}
