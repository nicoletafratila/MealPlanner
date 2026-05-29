using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Data.Entities;
using RecipeBook.Data.TableConfigurations;

namespace RecipeBook.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class ProductTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<Product>();

            var configuration = new ProductTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(Product));
            Assert.That(entityType, Is.Not.Null, "Product entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for Product.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<Product>();

            var configuration = new ProductTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
