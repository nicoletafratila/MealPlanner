using AutoMapper;
using Common.Data.DataContext;
using Common.Pagination;
using Identity.Api.Features.ApplicationUser.Queries.Search;
using Identity.Shared.Models;
using MealPlanner.Data.TableConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Data.TableConfigurations;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private ServiceProvider _provider = null!;
        private MealPlannerDbContext _context = null!;
        private UserManager<Data.Entities.ApplicationUser> _userManager = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

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
                options.UseInMemoryDatabase("ApplicationUserSearchTests_" + TestContext.CurrentContext.Test.ID));
            services.AddIdentity<Data.Entities.ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MealPlannerDbContext>();

            _provider = services.BuildServiceProvider();
            _context = _provider.GetRequiredService<MealPlannerDbContext>();
            _userManager = _provider.GetRequiredService<UserManager<Data.Entities.ApplicationUser>>();

            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_userManager, _mapperMock.Object);
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
                _ = new SearchQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new SearchQueryHandler(_userManager, null!));
        }

        [Test]
        public async Task Handle_NullQueryParameters_ReturnsEmptyPagedList()
        {
            var result = await _handler.Handle(new SearchQuery { QueryParameters = null }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }

            _mapperMock.Verify(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoUsers_ReturnsEmptyPagedList()
        {
            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns([]);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
        }

        [Test]
        public async Task Handle_ReturnsAllMappedUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", Email = "alice@example.com", IsActive = true },
                new() { Id = "2", UserName = "bob", Email = "bob@example.com", IsActive = false });

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", Email = "alice@example.com", IsActive = true },
                new() { UserId = "2", Username = "bob", Email = "bob@example.com", IsActive = false }
            };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(2));
                Assert.That(result.Items.Select(u => u.UserId), Is.EquivalentTo(["1", "2"]));
            }

            _mapperMock.Verify(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_TreatedAsEmptyList()
        {
            await SeedUsersAsync(new Data.Entities.ApplicationUser { Id = "1", UserName = "alice" });

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns((IList<ApplicationUserModel>)null!);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Is.Empty);
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Handle_WithNameFilter_ReturnsOnlyMatchingUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" });

            var models = new List<ApplicationUserModel> { new() { UserId = "1", Username = "alice" } };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var filter = new FilterItem(
                nameof(ApplicationUserModel.Username),
                "alice",
                FilterOperator.Contains,
                StringComparison.OrdinalIgnoreCase);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Filters = [filter],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].UserId, Is.EqualTo("1"));
            }
        }

        [Test]
        public async Task Handle_WithEmailFilter_ReturnsOnlyMatchingUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", Email = "alice@example.com" },
                new() { Id = "2", UserName = "bob", Email = "bob@example.com" });

            var models = new List<ApplicationUserModel> { new() { UserId = "1", Username = "alice", Email = "alice@example.com" } };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var filter = new FilterItem(
                nameof(ApplicationUserModel.Email),
                "alice",
                FilterOperator.Contains,
                StringComparison.OrdinalIgnoreCase);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Filters = [filter],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].UserId, Is.EqualTo("1"));
            }
        }

        [Test]
        public async Task Handle_WithIsActiveFilter_ReturnsOnlyActiveUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", IsActive = true },
                new() { Id = "2", UserName = "bob", IsActive = false });

            var models = new List<ApplicationUserModel> { new() { UserId = "1", Username = "alice", IsActive = true } };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var filter = new FilterItem(
                nameof(ApplicationUserModel.IsActive),
                "True",
                FilterOperator.Equals,
                StringComparison.OrdinalIgnoreCase);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Filters = [filter],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].UserId, Is.EqualTo("1"));
            }
        }

        [Test]
        public async Task Handle_WithIsActiveSortingAscending_ReturnsFalseBeforeTrue()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", IsActive = true },
                new() { Id = "2", UserName = "bob", IsActive = false });

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "2", Username = "bob", IsActive = false },
                new() { UserId = "1", Username = "alice", IsActive = true }
            };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Sorting =
                    [
                        new SortingModel { PropertyName = nameof(ApplicationUserModel.IsActive), Direction = SortDirection.Ascending }
                    ],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items[0].UserId, Is.EqualTo("2")); // IsActive = false first
                Assert.That(result.Items[1].UserId, Is.EqualTo("1")); // IsActive = true second
            }
        }

        [Test]
        public async Task Handle_WithIsLockedOutFilter_ReturnsOnlyLockedOutUsers()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10) },
                new() { Id = "2", UserName = "bob", LockoutEnd = null });

            var models = new List<ApplicationUserModel> { new() { UserId = "1", Username = "alice", IsLockedOut = true } };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var filter = new FilterItem(
                nameof(ApplicationUserModel.IsLockedOut),
                "True",
                FilterOperator.Equals,
                StringComparison.OrdinalIgnoreCase);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Filters = [filter],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].UserId, Is.EqualTo("1"));
            }
        }

        [Test]
        public async Task Handle_WithIsLockedOutSortingAscending_ReturnsFalseBeforeTrue()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice", LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10) },
                new() { Id = "2", UserName = "bob", LockoutEnd = null });

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "2", Username = "bob", IsLockedOut = false },
                new() { UserId = "1", Username = "alice", IsLockedOut = true }
            };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    Sorting =
                    [
                        new SortingModel { PropertyName = nameof(ApplicationUserModel.IsLockedOut), Direction = SortDirection.Ascending }
                    ],
                    PageNumber = 1,
                    PageSize = 10
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(2));
                Assert.That(result.Items[0].UserId, Is.EqualTo("2")); // IsLockedOut = false first
                Assert.That(result.Items[1].UserId, Is.EqualTo("1")); // IsLockedOut = true second
            }
        }

        [Test]
        public async Task Handle_WithPagination_ReturnsCorrectPage()
        {
            await SeedUsersAsync(
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" },
                new() { Id = "3", UserName = "carol" });

            var models = new List<ApplicationUserModel> { new() { UserId = "3", Username = "carol" } };

            _mapperMock
                .Setup(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()))
                .Returns(models);

            var result = await _handler.Handle(new SearchQuery
            {
                QueryParameters = new QueryParameters<ApplicationUserModel>
                {
                    PageNumber = 2,
                    PageSize = 2
                }
            }, CancellationToken.None);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Items, Has.Count.EqualTo(1));
                Assert.That(result.Items[0].UserId, Is.EqualTo("3"));
                Assert.That(result.Metadata.TotalCount, Is.EqualTo(3));
            }
        }
    }
}
