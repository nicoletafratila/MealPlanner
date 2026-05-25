using AutoMapper;
using BlazorBootstrap;
using Common.Pagination;
using Identity.Api.Features.ApplicationUser.Queries.Search;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.Search
{
    [TestFixture]
    public class SearchQueryHandlerTests
    {
        private Mock<UserManager<Common.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SearchQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Common.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _handler = new SearchQueryHandler(_userManagerMock.Object, _mapperMock.Object);
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
                _ = new SearchQueryHandler(_userManagerMock.Object, null!));
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

            _userManagerMock.Verify(m => m.Users, Times.Never);
            _mapperMock.Verify(m => m.Map<IList<ApplicationUserModel>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoUsers_ReturnsEmptyPagedList()
        {
            _userManagerMock
                .Setup(m => m.Users)
                .Returns(new List<Common.Data.Entities.ApplicationUser>().AsQueryable());

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
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice", Email = "alice@example.com", IsActive = true },
                new() { Id = "2", UserName = "bob",   Email = "bob@example.com",   IsActive = false }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", EmailAddress = "alice@example.com", IsActive = true },
                new() { UserId = "2", Username = "bob",   EmailAddress = "bob@example.com",   IsActive = false }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
            _userManagerMock
                .Setup(m => m.Users)
                .Returns(new List<Common.Data.Entities.ApplicationUser>().AsQueryable());

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
                Assert.That(result.Metadata.TotalCount, Is.Zero);
            }
        }

        [Test]
        public async Task Handle_WithNameFilter_ReturnsOnlyMatchingUsers()
        {
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice" },
                new() { UserId = "2", Username = "bob" }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
        public async Task Handle_WithIsActiveFilter_ReturnsOnlyActiveUsers()
        {
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", IsActive = true },
                new() { UserId = "2", Username = "bob",   IsActive = false }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", IsActive = true },
                new() { UserId = "2", Username = "bob",   IsActive = false }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", IsLockedOut = true },
                new() { UserId = "2", Username = "bob",   IsLockedOut = false }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice", IsLockedOut = true },
                new() { UserId = "2", Username = "bob",   IsLockedOut = false }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
            var entities = new List<Common.Data.Entities.ApplicationUser>
            {
                new() { Id = "1", UserName = "alice" },
                new() { Id = "2", UserName = "bob" },
                new() { Id = "3", UserName = "carol" }
            };

            var models = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice" },
                new() { UserId = "2", Username = "bob" },
                new() { UserId = "3", Username = "carol" }
            };

            _userManagerMock
                .Setup(m => m.Users)
                .Returns(entities.AsQueryable());

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
