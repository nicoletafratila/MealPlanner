using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using Common.UI;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipesOverviewTests
    {
        private const string EditBaseUrl = "recipebooks/recipeedit/";

        private BunitContext _ctx = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeModel>([], new Metadata()));

            _sessionStorageMock
                .Setup(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _recipeServiceMock.Reset();
            _sessionStorageMock.Reset();
            _messageComponentMock.Reset();
        }

        private IRenderedComponent<RecipesOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<RecipesOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- Navigation ----------
        [Test]
        public void New_NavigatesToCreatePage()
        {
            // Arrange
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            // Act
            cut.InvokeAsync(() =>
            {
                var m = typeof(RecipesOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(m, Is.Not.Null);
                m!.Invoke(cut.Instance, []);
            });

            // Assert
            Assert.That(navManager.Uri, Does.EndWith(EditBaseUrl));
        }

        [Test]
        public void Update_NavigatesToEditPage_ForGivenItem()
        {
            // Arrange
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new RecipeModel { Id = 42 };

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            // Assert
            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}42"));
        }

        // ---------- DeleteCoreAsync (core delete logic) ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshesGrid()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 5 };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(new CommandResponse { Succeeded = true, Message = "ok" });

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowInfoAsync("Data has been deleted successfully", It.IsAny<string>(), CancellationToken.None),
                Times.Once);

            _recipeServiceMock.Verify(s => s.DeleteAsync(recipe.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 7 };
            var response = new CommandResponse { Succeeded = false, Message = "delete failed" };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "DeleteCoreAsync method not found via reflection.");

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(m => m.ShowErrorAsync("delete failed", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
            _recipeServiceMock.Verify(s => s.DeleteAsync(recipe.Id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenResponseIsNull_ShowsGenericError()
        {
            // Arrange
            var recipe = new RecipeModel { Id = 9 };

            _recipeServiceMock
                .Setup(s => s.DeleteAsync(recipe.Id, CancellationToken.None))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [recipe])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(m => m.ShowErrorAsync("Delete failed. Please try again.", It.IsAny<string>(), It.IsAny<Exception>(), CancellationToken.None), Times.Once);
        }

        // ---------- DataProviderAsync ----------
        [Test]
        public async Task DataProviderAsync_CallsService_SavesQuery_InSessionStorage_AndReturnsData()
        {
            // Arrange
            var items = new List<RecipeModel>
            {
                new() { Id = 1 },
                new() { Id = 2 },
            };

            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            var paged = new PagedList<RecipeModel>(items, metadata);

            _recipeServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeModel>>(), CancellationToken.None))
                .ReturnsAsync(paged);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipesOverview).GetMethod("DataProviderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var request = new GridDataProviderRequest<RecipeModel>
            {
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            GridDataProviderResult<RecipeModel> result = await cut.InvokeAsync(async () =>
            {
                var task = (Task<GridDataProviderResult<RecipeModel>>)method!
                    .Invoke(cut.Instance, [request])!;
                return await task;
            });

            // Assert
            _recipeServiceMock.Verify(
                s => s.SearchAsync(It.Is<QueryParameters<RecipeModel>>(q =>
                    q.PageNumber == 1 && q.PageSize == 10), CancellationToken.None),
                Times.Exactly(2));

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Data!.Count(), Is.EqualTo(2));
                Assert.That(result.TotalCount, Is.EqualTo(2));
            }
        }
    }
}