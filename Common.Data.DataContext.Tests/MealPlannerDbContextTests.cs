using MealPlanner.Data.Entities;
using MealPlanner.Data.TableConfigurations;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.DataContext.Tests
{
    [TestFixture]
    public class MealPlannerDbContextTests
    {
        private static TableConfigurationAssemblies AllAssemblies() =>
            new([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]);

        private MealPlannerDbContext CreateContext(string dbName) =>
            new(
                new DbContextOptionsBuilder<MealPlannerDbContext>().UseInMemoryDatabase(dbName).Options,
                AllAssemblies()
            );

        [Test]
        public void Ctor_WithValidOptions_CreatesContext()
        {
            using var context = CreateContext(nameof(Ctor_WithValidOptions_CreatesContext));

            Assert.That(context, Is.Not.Null);
        }

        [Test]
        public void DbSets_AreNotNull()
        {
            using var context = CreateContext(nameof(DbSets_AreNotNull));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(context.MealPlans, Is.Not.Null);
                Assert.That(context.MealPlanRecipes, Is.Not.Null);
                Assert.That(context.Recipes, Is.Not.Null);
                Assert.That(context.RecipeIngredients, Is.Not.Null);
                Assert.That(context.Products, Is.Not.Null);
                Assert.That(context.ProductCategories, Is.Not.Null);
                Assert.That(context.RecipeCategories, Is.Not.Null);
                Assert.That(context.Units, Is.Not.Null);
                Assert.That(context.ShoppingLists, Is.Not.Null);
                Assert.That(context.ShoppingListProducts, Is.Not.Null);
                Assert.That(context.Shops, Is.Not.Null);
                Assert.That(context.ShopDisplaySequences, Is.Not.Null);
                Assert.That(context.Logs, Is.Not.Null);
            }
        }

        [Test]
        public void OnModelCreating_SetsQuantityPrecision_ForRecipeIngredient()
        {
            using var context = CreateContext(nameof(OnModelCreating_SetsQuantityPrecision_ForRecipeIngredient));

            var entityType = context.Model.FindEntityType(typeof(RecipeIngredient));
            Assert.That(entityType, Is.Not.Null, "RecipeIngredient entity not found in model.");

            var quantityProp = entityType!.FindProperty(nameof(RecipeIngredient.Quantity));
            Assert.That(quantityProp, Is.Not.Null, "Quantity property not found on RecipeIngredient.");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(quantityProp!.GetPrecision(), Is.EqualTo(18));
                Assert.That(quantityProp.GetScale(), Is.EqualTo(2));
            }
        }

        [Test]
        public void OnModelCreating_SetsQuantityPrecision_ForShoppingListProduct()
        {
            using var context = CreateContext(nameof(OnModelCreating_SetsQuantityPrecision_ForShoppingListProduct));

            var entityType = context.Model.FindEntityType(typeof(ShoppingListProduct));
            Assert.That(entityType, Is.Not.Null, "ShoppingListProduct entity not found in model.");

            var quantityProp = entityType!.FindProperty(nameof(ShoppingListProduct.Quantity));
            Assert.That(quantityProp, Is.Not.Null, "Quantity property not found on ShoppingListProduct.");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(quantityProp!.GetPrecision(), Is.EqualTo(18));
                Assert.That(quantityProp.GetScale(), Is.EqualTo(2));
            }
        }
    }
}
