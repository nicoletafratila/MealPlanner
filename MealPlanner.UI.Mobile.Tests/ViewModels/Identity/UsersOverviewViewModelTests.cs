using Common.Models;
using Common.Pagination;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Mobile.ViewModels.Identity;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.Identity
{
    [TestFixture]
    public class UsersOverviewViewModelTests
    {
        private Mock<IApplicationUserService> _userServiceMock = null!;
        private UsersOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);
            _viewModel = new UsersOverviewViewModel(_userServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_PopulatesUsersAndPagination()
        {
            var items = new List<ApplicationUserModel>
            {
                new() { UserId = "1", Username = "alice" },
                new() { UserId = "2", Username = "bob" }
            };
            var metadata = Metadata.Create(1, 20, 40);

            _userServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Users, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.HasPreviousPage, Is.False);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task SearchAsync_ResetsToFirstPageAndFiltersByUsername()
        {
            _viewModel.CurrentPage = 3;
            _viewModel.SearchText = "ali";
            var metadata = Metadata.Create(1, 20, 1);

            _userServiceMock
                .Setup(x => x.SearchAsync(
                    It.Is<QueryParameters<ApplicationUserModel>>(p =>
                        p.PageNumber == 1 &&
                        p.Filters != null &&
                        p.Filters.Any(f => f.PropertyName == "Username" && (string)f.Value! == "ali")),
                    CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([new ApplicationUserModel { UserId = "1", Username = "alice" }], metadata));

            await _viewModel.SearchCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(1));
                Assert.That(_viewModel.Users, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public async Task NextPageAsync_IncrementsPageAndReloads()
        {
            var firstMetadata = Metadata.Create(1, 20, 40);
            _userServiceMock
                .Setup(x => x.SearchAsync(It.Is<QueryParameters<ApplicationUserModel>>(p => p.PageNumber == 1), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([new ApplicationUserModel { UserId = "1", Username = "alice" }], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 20, 40);
            _userServiceMock
                .Setup(x => x.SearchAsync(It.Is<QueryParameters<ApplicationUserModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([new ApplicationUserModel { UserId = "2", Username = "bob" }], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.Users, Has.Count.EqualTo(1));
                Assert.That(_viewModel.Users[0].Username, Is.EqualTo("bob"));
            }
        }

        [Test]
        public async Task PreviousPageAsync_WhenOnFirstPage_DoesNotCallService()
        {
            _viewModel.CurrentPage = 1;

            await _viewModel.PreviousPageCommand.ExecuteAsync(null);

            _userServiceMock.Verify(x => x.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task PreviousPageAsync_WhenNotOnFirstPage_DecrementsPageAndReloads()
        {
            _viewModel.CurrentPage = 2;
            var metadata = Metadata.Create(1, 20, 40);
            _userServiceMock
                .Setup(x => x.SearchAsync(It.Is<QueryParameters<ApplicationUserModel>>(p => p.PageNumber == 1), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([new ApplicationUserModel { UserId = "1", Username = "alice" }], metadata));

            await _viewModel.PreviousPageCommand.ExecuteAsync(null);

            Assert.That(_viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public async Task UnlockUserAsync_Success_ReloadsList()
        {
            _userServiceMock
                .Setup(x => x.UnlockAsync("user-1", CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            var reloadMetadata = Metadata.Create(1, 20, 1);
            _userServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ApplicationUserModel>([new ApplicationUserModel { UserId = "user-1", Username = "alice", IsLockedOut = false }], reloadMetadata));

            await _viewModel.UnlockUserCommand.ExecuteAsync("user-1");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.Users, Has.Count.EqualTo(1));
            }
            _userServiceMock.Verify(x => x.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task UnlockUserAsync_Failure_SetsErrorMessageAndDoesNotReload()
        {
            _userServiceMock
                .Setup(x => x.UnlockAsync("user-1", CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("Unlock failed"));

            await _viewModel.UnlockUserCommand.ExecuteAsync("user-1");

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Unlock failed"));
            _userServiceMock.Verify(x => x.SearchAsync(It.IsAny<QueryParameters<ApplicationUserModel>>(), CancellationToken.None), Times.Never);
        }

        // OpenUserAsync calls Shell.Current.GoToAsync directly with no surrounding try/catch, so
        // it is not unit-testable in this host and is intentionally skipped.
    }
}
