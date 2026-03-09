using System.Data;
using Common.Data.DataContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Api.Tests.Migrations
{
    [TestFixture]
    public class InitialCreateMigrationTests
    {
        private SqliteConnection _connection = null!;
        private MealPlannerDbContext _context = null!;

        [SetUp]
        public async Task SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<MealPlannerDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new MealPlannerDbContext(options);

            await _context.Database.EnsureCreatedAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            await _context.DisposeAsync();
        }

        [Test]
        public async Task Database_CanBeCreated()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            Assert.That(canConnect, Is.True);
        }

        [Test]
        public void Tables_Exist_AfterEnsureCreated()
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
            SELECT name 
            FROM sqlite_master 
            WHERE type = 'table';
        ";

            using var reader = cmd.ExecuteReader();
            var tableNames = reader.Cast<IDataRecord>()
                .Select(r => r.GetString(0))
                .ToList();

            // Adjust names to what EF actually generates (case-sensitive in SQLite)
            using (Assert.EnterMultipleScope())
            {
                Assert.That(tableNames, Does.Contain("AspNetUsers"));
                Assert.That(tableNames, Does.Contain("AspNetRoles"));
                Assert.That(tableNames, Does.Contain("Logs"));
                Assert.That(tableNames, Does.Contain("MealPlans"));
                Assert.That(tableNames, Does.Contain("ProductCategories"));
                Assert.That(tableNames, Does.Contain("RecipeCategories"));
                Assert.That(tableNames, Does.Contain("Shops"));
                Assert.That(tableNames, Does.Contain("Units"));
                Assert.That(tableNames, Does.Contain("Recipes"));
                Assert.That(tableNames, Does.Contain("Products"));
                Assert.That(tableNames, Does.Contain("RecipeIngredients"));
                Assert.That(tableNames, Does.Contain("ShoppingLists"));
                Assert.That(tableNames, Does.Contain("ShoppingListProducts"));
                Assert.That(tableNames, Does.Contain("MealPlanRecipes"));
                Assert.That(tableNames, Does.Contain("ShopDisplaySequences"));
            }
        }

        [Test]
        public void Can_Insert_BasicEntities_AccordingToSchema()
        {
            var unit = new Common.Data.Entities.Unit
            {
                Name = "kg",
                UnitType = 0
            };

            var productCategory = new Common.Data.Entities.ProductCategory
            {
                Name = "Dairy"
            };

            var product = new Common.Data.Entities.Product
            {
                Name = "Milk",
                BaseUnit = unit,
                ProductCategory = productCategory
            };

            var recipeCategory = new Common.Data.Entities.RecipeCategory
            {
                Name = "Main",
                DisplaySequence = 1
            };

            var recipe = new Common.Data.Entities.Recipe
            {
                Name = "Recipe1",
                RecipeCategory = recipeCategory
            };

            var mealPlan = new Common.Data.Entities.MealPlan
            {
                Name = "Weekly Plan"
            };

            var mpr = new Common.Data.Entities.MealPlanRecipe
            {
                MealPlan = mealPlan,
                Recipe = recipe
            };

            var shop = new Common.Data.Entities.Shop
            {
                Name = "Shop1"
            };

            var shoppingList = new Common.Data.Entities.ShoppingList
            {
                Name = "List1",
                Shop = shop
            };

            var slProduct = new Common.Data.Entities.ShoppingListProduct
            {
                ShoppingList = shoppingList,
                Product = product,
                Unit = unit,
                Quantity = 1m,
                DisplaySequence = 1,
                Collected = false
            };

            var ingredient = new Common.Data.Entities.RecipeIngredient
            {
                Recipe = recipe,
                Product = product,
                Unit = unit,
                Quantity = 2m
            };

            _context.Units.Add(unit);
            _context.ProductCategories.Add(productCategory);
            _context.Products.Add(product);
            _context.RecipeCategories.Add(recipeCategory);
            _context.Recipes.Add(recipe);
            _context.MealPlans.Add(mealPlan);
            _context.MealPlanRecipes.Add(mpr);
            _context.Shops.Add(shop);
            _context.ShoppingLists.Add(shoppingList);
            _context.ShoppingListProducts.Add(slProduct);
            _context.RecipeIngredients.Add(ingredient);

            _context.SaveChanges();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_context.Units.Count(), Is.EqualTo(1));
                Assert.That(_context.Products.Count(), Is.EqualTo(1));
                Assert.That(_context.Recipes.Count(), Is.EqualTo(1));
                Assert.That(_context.MealPlans.Count(), Is.EqualTo(1));
                Assert.That(_context.ShoppingLists.Count(), Is.EqualTo(1));
            }
        }
    }
}