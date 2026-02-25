using System.Reflection;
using Blazored.Modal;
using Bunit;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Pages;
using MealPlanner.UI.Web.Pages.MealPlans;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages.MealPlans
{
    [TestFixture]
    public class ShopSelectionTests
    {
        private BunitContext _ctx = null!;
        private Mock<IShopService> _shopServiceMock = null!;
        private Mock<IModalController> _modalControllerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _shopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _modalControllerMock = new Mock<IModalController>(MockBehavior.Strict);

            _ctx.Services.AddSingleton(_shopServiceMock.Object);
            _ctx.Services.AddBlazoredModal();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _shopServiceMock.Reset();
            _modalControllerMock.Reset();
        }

        private IRenderedComponent<ShopSelection> RenderComponent()
        {
            return _ctx.Render<ShopSelection>();
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_LoadsShops()
        {
            // Arrange
            var shopItems = new List<ShopModel>
            {
                new() { Id = 1, Name = "Shop1" },
                new() { Id = 2, Name = "Shop2" }
            };

            var shops = new PagedList<ShopModel>(
                shopItems, new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(shops);

            // Act
            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            // Assert
            Assert.That(cut.Instance.Shops, Is.Not.Null);
            Assert.That(cut.Instance.Shops!.Items, Has.Count.EqualTo(2));

            _shopServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()),
                Times.Once);
        }

        // ---------- SaveAsync / CancelAsync using IModalController mock ----------
        [Test]
        public async Task SaveAsync_UsesModalController_WithShopId()
        {
            // Arrange
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(new PagedList<ShopModel>(new List<ShopModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CloseAsync(It.IsAny<object?>()))
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();

            cut.Instance.ModalController = _modalControllerMock.Object;
            cut.Instance.ShopId = "42";

            var method = typeof(ShopSelection).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(
                m => m.CloseAsync("42"),
                Times.Once);
        }

        [Test]
        public async Task CancelAsync_UsesModalController_Cancel()
        {
            // Arrange
            _shopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>()))
                .ReturnsAsync(new PagedList<ShopModel>(new List<ShopModel>(), new Metadata()));

            _modalControllerMock
                .Setup(m => m.CancelAsync())
                .Returns(Task.CompletedTask);

            var cut = RenderComponent();
            cut.Instance.ModalController = _modalControllerMock.Object;

            var method = typeof(ShopSelection).GetMethod("CancelAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, Array.Empty<object>())!;
                await task;
            });

            // Assert
            _modalControllerMock.Verify(m => m.CancelAsync(), Times.Once);
        }
    }
}