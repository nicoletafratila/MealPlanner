using MealPlanner.Data.Entities;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class ShopTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<Shop>();

            var configuration = new ShopTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(Shop));
            Assert.That(entityType, Is.Not.Null, "Shop entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for Shop.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<Shop>();

            var configuration = new ShopTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
