using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.Repository.Tests
{
    [TestFixture]
    public class BaseAsyncRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<TestDbContext>(options => 
                options.UseInMemoryDatabase("BaseAsyncRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private BaseAsyncRepository<TestEntity, int> CreateRepository(out TestDbContext context)
        {
            context = _provider.GetRequiredService<TestDbContext>();
            return new BaseAsyncRepository<TestEntity, int>(context);
        }

        [Test]
        public async Task AddAsync_PersistsEntity()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var entity = new TestEntity
            {
                Name = "Entity1"
            };

            // Act
            var added = await repo.AddAsync(entity);

            // Assert
            Assert.That(added.Id, Is.Not.EqualTo(0));
            Assert.That(ctx.TestEntities.Count(), Is.EqualTo(1));
            Assert.That(ctx.TestEntities.Single().Name, Is.EqualTo("Entity1"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsEntity_WhenExists()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var entity = new TestEntity { Name = "Entity1" };
            ctx.TestEntities.Add(entity);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(entity.Id);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(entity.Id));
            Assert.That(found.Name, Is.EqualTo("Entity1"));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var repo = CreateRepository(out _);

            // Act
            var found = await repo.GetByIdAsync(999);

            // Assert
            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.TestEntities.AddRange(
                new TestEntity { Name = "E1" },
                new TestEntity { Name = "E2" });
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync();

            // Assert
            Assert.That(all, Is.Not.Null);
            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.Select(e => e.Name), Is.EquivalentTo(["E1", "E2"]));
        }

        [Test]
        public async Task UpdateAsync_UpdatesEntity()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var entity = new TestEntity { Name = "Old" };
            ctx.TestEntities.Add(entity);
            await ctx.SaveChangesAsync();

            entity.Name = "New";

            // Act
            await repo.UpdateAsync(entity);

            // Assert
            var fromDb = await ctx.TestEntities.FindAsync(entity.Id);
            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb!.Name, Is.EqualTo("New"));
        }

        [Test]
        public async Task DeleteAsync_RemovesEntity()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var entity = new TestEntity { Name = "ToDelete" };
            ctx.TestEntities.Add(entity);
            await ctx.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(entity);

            // Assert
            Assert.That(ctx.TestEntities.Any(), Is.False);
        }

        [Test]
        public void AddAsync_ThrowsArgumentNullException_WhenEntityNull()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await repo.AddAsync(null!);
            });
        }

        [Test]
        public void UpdateAsync_ThrowsArgumentNullException_WhenEntityNull()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await repo.UpdateAsync(null!);
            });
        }

        [Test]
        public void DeleteAsync_ThrowsArgumentNullException_WhenEntityNull()
        {
            var repo = CreateRepository(out _);

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await repo.DeleteAsync(null!);
            });
        }
    }
}