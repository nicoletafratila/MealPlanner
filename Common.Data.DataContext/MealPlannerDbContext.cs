using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Common.Data.DataContext
{
    public class MealPlannerDbContext : DbContext
    {
        public MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : base(options)
        {

        }

        public DbSet<MealPlan> MealPlans => Set<MealPlan>();
        public DbSet<MealPlanRecipe> MealPlanRecipes => Set<MealPlanRecipe>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<IngredientCategory> IngredientCategories => Set<IngredientCategory>();
        public DbSet<RecipeCategory> RecipeCategories => Set<RecipeCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //seed units of measurements
            //modelBuilder.Entity<Unit>().HasData(new Country { CountryId = 1, Name = "Belgium" });

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
