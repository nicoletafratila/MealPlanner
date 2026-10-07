using System.Reflection;
using Blazored.Modal;
using Bunit;
using Identity.Services.Http;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Pages;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipeShareUserSelectionTests
    {
        private BunitContext _ctx = null!;
        private Mock<IApplicationUserService> _applicationUserServiceMock = null!;
        private Mock<IModalController> _modalControllerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _applicationUserServiceMock = new Mock<IApplicationUserService>(MockBehavior.Strict);
            _modalControllerMock = new Mock<IModalController>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_applicationUserServiceMock.Object);
            _ctx.Services.AddBlazoredModal();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<RecipeShareUserSelection> RenderComponent()
        {
            return _ctx.Render<RecipeShareUserSelection>();
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_LoadsUsers()
        {
            // Arrange
            var users = new List<ApplicationUserListModel>
            {
                new() { UserId = "2", Username = "bob", FirstName = "Bob", LastName = "Smith" }
            };

            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync(users);

            // Act
            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            // Assert
            Assert.That(cut.Instance.Users, Is.Not.Null);
            Assert.That(cut.Instance.Users!, Has.Count.EqualTo(1));

            _applicationUserServiceMock.Verify(s => s.ListAsync(CancellationToken.None), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_NullFromService_FallsBackToEmptyList()
        {
            // Arrange
            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync((IList<ApplicationUserListModel>?)null);

            // Act
            var cut = RenderComponent();

            // Assert
            Assert.That(cut.Instance.Users, Is.Not.Null);
            Assert.That(cut.Instance.Users!, Is.Empty);
        }

        // ---------- SaveAsync / CancelAsync ----------
        [Test]
        public async Task SaveAsync_UsesModalController_WithTargetUserId()
        {
            // Arrange
            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync([]);

            _modalControllerMock
                .Setup(m => m.CloseAsync(It.IsAny<object?>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;
            cut.Instance.TargetUserId = "user2";

            var method = typeof(RecipeShareUserSelection).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(m => m.CloseAsync("user2"), Times.Once);
        }

        [Test]
        public async Task CancelAsync_UsesModalController_Cancel()
        {
            // Arrange
            _applicationUserServiceMock
                .Setup(s => s.ListAsync(CancellationToken.None))
                .ReturnsAsync([]);

            _modalControllerMock
                .Setup(m => m.CancelAsync())
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(RecipeShareUserSelection).GetMethod("CancelAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(m => m.CancelAsync(), Times.Once);
        }

        // ---------- DisplayName ----------
        [Test]
        public void DisplayName_WithFullName_ReturnsNameAndUsername()
        {
            var method = typeof(RecipeShareUserSelection).GetMethod("DisplayName", BindingFlags.Static | BindingFlags.NonPublic)!;
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob", FirstName = "Bob", LastName = "Smith" };

            var result = (string)method.Invoke(null, [user])!;

            Assert.That(result, Is.EqualTo("Bob Smith (bob)"));
        }

        [Test]
        public void DisplayName_WithoutName_ReturnsUsernameOnly()
        {
            var method = typeof(RecipeShareUserSelection).GetMethod("DisplayName", BindingFlags.Static | BindingFlags.NonPublic)!;
            var user = new ApplicationUserListModel { UserId = "1", Username = "bob" };

            var result = (string)method.Invoke(null, [user])!;

            Assert.That(result, Is.EqualTo("bob"));
        }
    }
}
