using Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Identity.Data.TableConfigurations.Tests
{
    [TestFixture]
    public class RefreshTokenTableConfigurationTests
    {
        [Test]
        public void Configure_SetsUniqueTokenHashIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<RefreshToken>();

            var configuration = new RefreshTokenTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(RefreshToken));
            Assert.That(entityType, Is.Not.Null, "RefreshToken entity not found in model.");

            var tokenHashIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "TokenHash"));
            Assert.That(tokenHashIndex, Is.Not.Null, "Index on TokenHash not configured for RefreshToken.");
            Assert.That(tokenHashIndex!.IsUnique, Is.True, "Index on TokenHash should be unique.");
        }

        [Test]
        public void Configure_SetsUserIdIndex()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            var entityBuilder = modelBuilder.Entity<RefreshToken>();

            var configuration = new RefreshTokenTableConfiguration();
            configuration.Configure(entityBuilder);

            var entityType = modelBuilder.Model.FindEntityType(typeof(RefreshToken));
            Assert.That(entityType, Is.Not.Null, "RefreshToken entity not found in model.");

            var userIdIndex = entityType!.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "UserId"));
            Assert.That(userIdIndex, Is.Not.Null, "Index on UserId not configured for RefreshToken.");
            Assert.That(userIdIndex!.IsUnique, Is.False, "Index on UserId should not be unique.");
        }

        [Test]
        public void Configure_DoesNotThrow()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);
            var entityBuilder = modelBuilder.Entity<RefreshToken>();

            var configuration = new RefreshTokenTableConfiguration();

            Assert.That(() => configuration.Configure(entityBuilder), Throws.Nothing);
        }
    }
}
