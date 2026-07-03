using MealPlanner.Data.Entities;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class ShoppingListProductTableConfigurationTests
    {
        [Test]
        public void Configure_SetsCompositePrimaryKey()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<ShoppingListProduct>();

            var configuration = new ShoppingListProductTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(ShoppingListProduct));
            Assert.That(entityType, Is.Not.Null, "ShoppingListProduct entity not found in model.");

            var key = entityType!.FindPrimaryKey();
            Assert.That(key, Is.Not.Null, "Primary key not configured for ShoppingListProduct.");

            var keyPropertyNames = key!.Properties.Select(p => p.Name).ToArray();
            Assert.That(keyPropertyNames, Is.EqualTo(new[] { "ShoppingListId", "ProductId" }));
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<ShoppingListProduct>();

            var configuration = new ShoppingListProductTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
