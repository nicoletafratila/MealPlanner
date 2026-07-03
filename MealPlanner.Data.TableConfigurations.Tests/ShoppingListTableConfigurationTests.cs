using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MealPlanner.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class ShoppingListTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<ShoppingList>();

            var configuration = new ShoppingListTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(ShoppingList));
            Assert.That(entityType, Is.Not.Null, "ShoppingList entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for ShoppingList.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<ShoppingList>();

            var configuration = new ShoppingListTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
