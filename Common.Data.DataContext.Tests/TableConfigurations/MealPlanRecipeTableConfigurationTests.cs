using Common.Data.DataContext.TableConfigurations;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Common.Data.DataContext.Tests.TableConfigurations
{
    [TestFixture]
    public class MealPlanRecipeTableConfigurationTests
    {
        [Test]
        public void Configure_SetsCompositePrimaryKey()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<MealPlanRecipe>();

            var configuration = new MealPlanRecipeTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(MealPlanRecipe));
            Assert.That(entityType, Is.Not.Null, "MealPlanRecipe entity not found in model.");

            var key = entityType!.FindPrimaryKey();
            Assert.That(key, Is.Not.Null, "Primary key not configured for MealPlanRecipe.");

            var keyPropertyNames = key!.Properties.Select(p => p.Name).ToArray();
            Assert.That(keyPropertyNames, Is.EqualTo(new[] { "MealPlanId", "RecipeId" }));
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<MealPlanRecipe>();

            var configuration = new MealPlanRecipeTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}