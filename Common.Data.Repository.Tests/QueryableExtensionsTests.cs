using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Data.Repository.Tests
{
    [TestFixture]
    public class QueryableExtensionsTests
    {
        private ServiceProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddDbContext<TestDbContext>(options =>
                options.UseInMemoryDatabase("QueryableExtensionsTests_" + TestContext.CurrentContext.Test.ID));

            _provider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private TestDbContext CreateContext() => _provider.GetRequiredService<TestDbContext>();

        private static async Task SeedAsync(TestDbContext context, int count)
        {
            for (var i = 1; i <= count; i++)
            {
                context.TestEntities.Add(new TestEntity { Name = $"Entity{i}" });
            }
            await context.SaveChangesAsync();
        }

        [Test]
        public async Task ToPagedResultAsync_AllItemsFitOnPage_ReturnsExactTotal()
        {
            var context = CreateContext();
            await SeedAsync(context, 5);

            var result = await context.TestEntities.OrderBy(e => e.Id).ToPagedResultAsync(1, 100, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(5));
                Assert.That(result.TotalCount, Is.EqualTo(5));
                Assert.That(result.Skip, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ToPagedResultAsync_EmptySource_ReturnsZeroTotal()
        {
            var context = CreateContext();

            var result = await context.TestEntities.OrderBy(e => e.Id).ToPagedResultAsync(1, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.TotalCount, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ToPagedResultAsync_LastPagePartiallyFilled_ReturnsExactTotal()
        {
            var context = CreateContext();
            await SeedAsync(context, 25);

            var result = await context.TestEntities.OrderBy(e => e.Id).ToPagedResultAsync(3, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(5));
                Assert.That(result.TotalCount, Is.EqualTo(25));
                Assert.That(result.Skip, Is.EqualTo(20));
            }
        }

        [Test]
        public async Task ToPagedResultAsync_MultiplePagesRemain_ReturnsCorrectPageAndTotal()
        {
            var context = CreateContext();
            await SeedAsync(context, 25);

            var result = await context.TestEntities.OrderBy(e => e.Id).ToPagedResultAsync(1, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(10));
                Assert.That(result.Items.Select(e => e.Name), Is.EqualTo(Enumerable.Range(1, 10).Select(i => $"Entity{i}")));
                Assert.That(result.TotalCount, Is.EqualTo(25));
                Assert.That(result.Skip, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ToPagedResultAsync_PageNumberPastLastPage_ReturnsEmptyItemsWithAccurateTotal()
        {
            var context = CreateContext();
            await SeedAsync(context, 5);

            var result = await context.TestEntities.OrderBy(e => e.Id).ToPagedResultAsync(10, 10, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.TotalCount, Is.EqualTo(5));
                Assert.That(result.Skip, Is.EqualTo(90));
            }
        }

        [Test]
        public void ToPagedResultAsync_PageNumberAndSizeOverflow_ThrowsOverflowException()
        {
            var context = CreateContext();

            Assert.ThrowsAsync<OverflowException>(async () =>
                await context.TestEntities.ToPagedResultAsync(int.MaxValue, int.MaxValue, CancellationToken.None));
        }

        [Test]
        public void ToPagedResultAsync_NullSource_Throws()
        {
            IQueryable<TestEntity>? source = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await source!.ToPagedResultAsync(1, 10, CancellationToken.None));
        }
    }
}
