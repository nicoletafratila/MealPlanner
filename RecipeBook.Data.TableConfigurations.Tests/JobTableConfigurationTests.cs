using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using RecipeBook.Data.Entities;

namespace RecipeBook.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class JobTableConfigurationTests
    {
        [Test]
        public void Configure_SetsOwnerUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<Job>();

            var configuration = new JobTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(Job));
            Assert.That(entityType, Is.Not.Null, "Job entity not found in model.");

            var ownerUserIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "OwnerUserId"));
            Assert.That(ownerUserIdIndex, Is.Not.Null, "Index on OwnerUserId not configured for Job.");
        }

        [Test]
        public void Configure_SetsStatusIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<Job>();

            var configuration = new JobTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(Job));
            Assert.That(entityType, Is.Not.Null, "Job entity not found in model.");

            var statusIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "Status"));
            Assert.That(statusIndex, Is.Not.Null, "Index on Status not configured for Job.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<Job>();

            var configuration = new JobTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
