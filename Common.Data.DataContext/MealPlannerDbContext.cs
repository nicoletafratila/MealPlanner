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
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<RecipeCategory> RecipeCategories => Set<RecipeCategory>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
        public DbSet<ShoppingListProduct> ShoppingListProducts => Set<ShoppingListProduct>();
        public DbSet<Shop> Shops => Set<Shop>();
        public DbSet<ShopDisplaySequence> ShopDisplaySequences => Set<ShopDisplaySequence>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
