using Microsoft.EntityFrameworkCore;
using RecipeBook.Api.Data.Entities;
using System.Reflection;

namespace RecipeBook.Api.Data
{
    public class MealPlannerDbContext : DbContext
    {
        public MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : base(options)
        {

        }

        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealPlanRecipe> MealPlanRecipes { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //seed units of measurements
            //modelBuilder.Entity<Unit>().HasData(new Country { CountryId = 1, Name = "Belgium" });

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
