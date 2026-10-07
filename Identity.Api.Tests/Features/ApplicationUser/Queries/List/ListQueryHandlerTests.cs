using Common.Data.DataContext;
using Common.Services;
using Identity.Api.Features.ApplicationUser.Queries.List;
using MealPlanner.Data.TableConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Data.TableConfigurations;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.List
{
    [TestFixture]
    public class ListQueryHandlerTests
    {
        private ServiceProvider _provider = null!;
        private MealPlannerDbContext _context = null!;
        private UserManager<Data.Entities.ApplicationUser> _userManager = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private ListQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly
            ]));
            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseInMemoryDatabase("ApplicationUserListTests_" + TestContext.CurrentContext.Test.ID));
            services.AddIdentity<Data.Entities.ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MealPlannerDbContext>();

            _provider = services.BuildServiceProvider();
            _context = _provider.GetRequiredService<MealPlannerDbContext>();
            _userManager = _provider.GetRequiredService<UserManager<Data.Entities.ApplicationUser>>();

            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _handler = new ListQueryHandler(_userManager, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _userManager.Dispose();
            _context.Dispose();
            _provider.Dispose();
        }

        private async Task SeedUsersAsync(params Data.Entities.ApplicationUser[] users)
        {
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }

        [Test]
        public void Ctor_NullUserManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ListQueryHandler(null!, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ListQueryHandler(_userManager, null!));
        }

        [Test]
        public async Task Handle_ExcludesCurrentUser()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", IsActive = true },
                new() { Id = "2", UserName = "bob", IsActive = true });

            _currentUserMock.Setup(s => s.UserId).Returns("1");

            var result = await _handler.Handle(new ListQuery(), CancellationToken.None);

            Assert.That(result.Select(u => u.UserId), Is.EquivalentTo(["2"]));
        }

        [Test]
        public async Task Handle_ExcludesInactiveUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", IsActive = true },
                new() { Id = "2", UserName = "bob", IsActive = false });

            _currentUserMock.Setup(s => s.UserId).Returns("3");

            var result = await _handler.Handle(new ListQuery(), CancellationToken.None);

            Assert.That(result.Select(u => u.UserId), Is.EquivalentTo(["1"]));
        }

        [Test]
        public async Task Handle_ReturnsOrderedByUsername_WithExpectedFields()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "bob", FirstName = "Bob", LastName = "Smith", IsActive = true },
                new() { Id = "2", UserName = "alice", FirstName = "Alice", LastName = "Jones", IsActive = true });

            _currentUserMock.Setup(s => s.UserId).Returns("3");

            var result = await _handler.Handle(new ListQuery(), CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Select(u => u.Username), Is.EqualTo(["alice", "bob"]));
                Assert.That(result[0].FirstName, Is.EqualTo("Alice"));
                Assert.That(result[0].LastName, Is.EqualTo("Jones"));
            }
        }

        [Test]
        public async Task Handle_NoOtherActiveUsers_ReturnsEmptyList()
        {
            await SeedUsersAsync(new Data.Entities.ApplicationUser { Id = "1", UserName = "alice", IsActive = true });

            _currentUserMock.Setup(s => s.UserId).Returns("1");

            var result = await _handler.Handle(new ListQuery(), CancellationToken.None);

            Assert.That(result, Is.Empty);
        }
    }
}
