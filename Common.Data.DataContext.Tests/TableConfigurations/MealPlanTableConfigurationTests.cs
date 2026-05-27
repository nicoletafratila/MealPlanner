using Common.Data.DataContext.TableConfigurations;
using MealPlanner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Common.Data.DataContext.Tests.TableConfigurations
{
    [TestFixture]
    public class MealPlanTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<MealPlan>();

            var configuration = new MealPlanTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(MealPlan));
            Assert.That(entityType, Is.Not.Null, "MealPlan entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for MealPlan.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<MealPlan>();

            var configuration = new MealPlanTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
