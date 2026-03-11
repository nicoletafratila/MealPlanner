using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.Repository.Tests
{
    [TestFixture]
    public class LoggerRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("LoggerRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<ILoggerRepository, LoggerRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private LoggerRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new LoggerRepository(context);
        }

        [Test]
        public async Task AddAsync_PersistsLogEntry()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var log = new Log
            {
                 Message = "Test log entry",
                 Level = "Info"
            };

            // Act
            var added = await repo.AddAsync(log, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(added.Id, Is.Not.Zero);
                Assert.That(ctx.Set<Log>().Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllLogs()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            ctx.Set<Log>().Add(new Log());
            ctx.Set<Log>().Add(new Log());
            await ctx.SaveChangesAsync();

            // Act
            var all = await repo.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.That(all, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task GetByIdAsync_ReturnsLog_WhenExists()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var log = new Log();
            ctx.Set<Log>().Add(log);
            await ctx.SaveChangesAsync();

            // Act
            var found = await repo.GetByIdAsync(log.Id, CancellationToken.None);

            // Assert
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(log.Id));
        }

        [Test]
        public async Task DeleteAsync_RemovesLog()
        {
            // Arrange
            var repo = CreateRepository(out var ctx);

            var log = new Log();
            ctx.Set<Log>().Add(log);
            await ctx.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(log, CancellationToken.None);

            // Assert
            Assert.That(ctx.Set<Log>().Any(), Is.False);
        }
    }
}