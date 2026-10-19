using Common.Data.DataContext;
using Common.Pagination;
using MealPlanner.Data.TableConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;
using RecipeBook.Data.TableConfigurations;

namespace RecipeBook.Api.Tests.Repositories
{
    [TestFixture]
    public class JobRepositoryTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));

            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("JobRepositoryTests_" + TestContext.CurrentContext.Test.ID));

            services.AddScoped<JobRepository>();

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private JobRepository CreateRepository(out MealPlannerDbContext context)
        {
            context = _provider.GetRequiredService<MealPlannerDbContext>();
            return new JobRepository(context);
        }

        private static Job CreateJob(JobStatus status, string ownerUserId = "user1", DateTime? createdAt = null) => new()
        {
            OwnerUserId = ownerUserId,
            TargetUserId = "user2",
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

        [Test]
        public async Task GetPendingAsync_ReturnsOnlyPendingJobs()
        {
            var repo = CreateRepository(out var ctx);

            ctx.Jobs.AddRange(
                CreateJob(JobStatus.Pending),
                CreateJob(JobStatus.Running),
                CreateJob(JobStatus.Completed));
            await ctx.SaveChangesAsync();

            var result = await repo.GetPendingAsync(CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Status, Is.EqualTo(JobStatus.Pending));
        }

        [Test]
        public async Task GetOrphanedRunningAsync_ReturnsOnlyRunningJobs()
        {
            var repo = CreateRepository(out var ctx);

            ctx.Jobs.AddRange(
                CreateJob(JobStatus.Running),
                CreateJob(JobStatus.Pending),
                CreateJob(JobStatus.Failed));
            await ctx.SaveChangesAsync();

            var result = await repo.GetOrphanedRunningAsync(CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Status, Is.EqualTo(JobStatus.Running));
        }

        [Test]
        public async Task SearchAsync_NoFilters_ReturnsAllJobsAcrossUsers()
        {
            var repo = CreateRepository(out var ctx);

            ctx.Jobs.AddRange(
                CreateJob(JobStatus.Completed, "user1"),
                CreateJob(JobStatus.Failed, "user2"));
            await ctx.SaveChangesAsync();

            var (items, totalCount, skip) = await repo.SearchAsync(null, null, 1, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Has.Count.EqualTo(2));
                Assert.That(totalCount, Is.EqualTo(2));
                Assert.That(skip, Is.Zero);
            }
        }

        [Test]
        public async Task SearchAsync_StatusFilter_ReturnsOnlyMatchingStatus()
        {
            var repo = CreateRepository(out var ctx);

            ctx.Jobs.AddRange(
                CreateJob(JobStatus.Completed),
                CreateJob(JobStatus.Failed));
            await ctx.SaveChangesAsync();

            var filters = new[] { new FilterItem("Status", JobStatus.Failed.ToString(), FilterOperator.Equals) };

            var (items, totalCount, _) = await repo.SearchAsync(filters, null, 1, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(1));
                Assert.That(items.Single().Status, Is.EqualTo(JobStatus.Failed));
            }
        }

        [Test]
        public async Task SearchAsync_Sorting_ReturnsSortedByRequestedProperty()
        {
            var repo = CreateRepository(out var ctx);

            var older = CreateJob(JobStatus.Completed, createdAt: DateTime.UtcNow.AddDays(-1));
            var newer = CreateJob(JobStatus.Completed, createdAt: DateTime.UtcNow);
            ctx.Jobs.AddRange(newer, older);
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = "CreatedAt", Direction = SortDirection.Ascending } };

            var (items, _, _) = await repo.SearchAsync(null, sorting, 1, 10, CancellationToken.None);

            Assert.That(items.Select(x => x.Id), Is.EqualTo(new[] { older.Id, newer.Id }));
        }

        [Test]
        public async Task SearchAsync_Paging_ReturnsRequestedPageAndSkip()
        {
            var repo = CreateRepository(out var ctx);

            for (var i = 0; i < 5; i++)
                ctx.Jobs.Add(CreateJob(JobStatus.Completed, createdAt: DateTime.UtcNow.AddMinutes(i)));
            await ctx.SaveChangesAsync();

            var sorting = new[] { new SortingModel { PropertyName = "CreatedAt", Direction = SortDirection.Ascending } };

            var (items, totalCount, skip) = await repo.SearchAsync(null, sorting, 2, 2, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(totalCount, Is.EqualTo(5));
                Assert.That(skip, Is.EqualTo(2));
                Assert.That(items, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public async Task SearchAsync_NoMatches_ReturnsEmptyWithZeroTotalCount()
        {
            var repo = CreateRepository(out _);

            var (items, totalCount, _) = await repo.SearchAsync(null, null, 1, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Is.Empty);
                Assert.That(totalCount, Is.Zero);
            }
        }
    }
}
