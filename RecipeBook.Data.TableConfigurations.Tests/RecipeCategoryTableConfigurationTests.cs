using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class RecipeCategoryTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<RecipeCategory>();

            var configuration = new RecipeCategoryTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(RecipeCategory));
            Assert.That(entityType, Is.Not.Null, "RecipeCategory entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for RecipeCategory.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<RecipeCategory>();

            var configuration = new RecipeCategoryTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
