using System.Reflection;
using BlazorBootstrap;
using Bunit;
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
    public class RecipeCategoriesOverviewTests
    {
        private const string EditBaseUrl = "recipebooks/recipecategoryedit/";

        private BunitContext _ctx = null!;
        private Mock<IRecipeCategoryService> _serviceMock = null!;
        private Mock<IMessageComponent> _messageMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _serviceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _messageMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_serviceMock.Object);

            _ctx.Services.AddScoped<BreadcrumbService>();
            _ctx.Services.AddScoped<ModalService>();
            _ctx.Services.AddScoped<PreloadService>();

            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.dropdown.initialize", _ => true);
            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.confirmDialog.show", _ => true);

            _serviceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _serviceMock.Reset();
            _messageMock.Reset();
        }

        private IRenderedComponent<RecipeCategoriesOverview> RenderWithMessageComponent()
        {
            return _ctx.Render<RecipeCategoriesOverview>(parameters =>
            {
                parameters.AddCascadingValue("MessageComponent", _messageMock.Object);
            });
        }

        // ---------- Navigation ----------
        [Test]
        public void New_NavigatesToEditPage_WithNoId()
        {
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            cut.InvokeAsync(() =>
            {
                var m = typeof(RecipeCategoriesOverview).GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(m, Is.Not.Null);
                m!.Invoke(cut.Instance, []);
            });

            Assert.That(navManager.Uri, Does.EndWith(EditBaseUrl));
        }

        [Test]
        public void Update_NavigatesToEditPage_WithId()
        {
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();
            var cut = RenderWithMessageComponent();

            var method = typeof(RecipeCategoriesOverview).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var model = new RecipeCategoryModel { Id = 42 };

            cut.InvokeAsync(() => method!.Invoke(cut.Instance, [model]));

            Assert.That(navManager.Uri, Does.EndWith($"{EditBaseUrl}42"));
        }

        // ---------- DeleteCoreAsync ----------
        [Test]
        public async Task DeleteCoreAsync_WhenDeleteSucceeds_ShowsInfo_AndRefreshes()
        {
            var category = new RecipeCategoryModel { Id = 5 };

            _serviceMock
                .Setup(s => s.DeleteAsync(category.Id))
                .ReturnsAsync(new Common.Models.CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipeCategoriesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowInfo("Data has been deleted successfully"), Times.Once);
            _serviceMock.Verify(s => s.DeleteAsync(category.Id), Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_WhenDeleteFails_ShowsError()
        {
            var category = new RecipeCategoryModel { Id = 7 };
            var response = new Common.Models.CommandResponse
            {
                Succeeded = false,
                Message = "delete failed"
            };

            _serviceMock
                .Setup(s => s.DeleteAsync(category.Id))
                .ReturnsAsync(response);

            var cut = RenderWithMessageComponent();

            var method = typeof(RecipeCategoriesOverview).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowError("delete failed"), Times.Once);
        }

        // ---------- SaveAsync ----------
        [Test]
        public async Task SaveAsync_WhenSaveSucceeds_ShowsInfo_AndRefreshes()
        {
            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            _serviceMock
              .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
              .ReturnsAsync(new PagedList<RecipeCategoryModel>(
                  categories, new Metadata()));

            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<IList<RecipeCategoryModel>>()))
                .ReturnsAsync(new Common.Models.CommandResponse { Succeeded = true });

            var cut = RenderWithMessageComponent();

            var saveMethod = typeof(RecipeCategoriesOverview).GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(saveMethod, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)saveMethod!.Invoke(cut.Instance, [])!;
                await task;
            });

            _messageMock.Verify(m => m.ShowInfo("Data has been saved successfully"), Times.Once);
            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<IList<RecipeCategoryModel>>()), Times.Once);
        }

        // ---------- MoveUp / MoveDown ----------
        [Test]
        public void MoveUp_MovesItemUpAndUpdatesIndexes()
        {
            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };

            _serviceMock
              .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
              .ReturnsAsync(new PagedList<RecipeCategoryModel>(
                  categories, new Metadata()));

            var cut = RenderWithMessageComponent();

            var moveUp = typeof(RecipeCategoriesOverview).GetMethod(
                "MoveUp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(moveUp, Is.Not.Null);

            var secondItem = categories[1];

            cut.InvokeAsync(() => moveUp!.Invoke(cut.Instance, [secondItem]));

            // After move up, secondItem should be at index 0
            var instanceCategories = cut.Instance.Categories;
            Assert.That(instanceCategories[0].Id, Is.EqualTo(secondItem.Id));
        }

        [Test]
        public void MoveDown_MovesItemDownAndUpdatesIndexes()
        {
            var categories = new List<RecipeCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 }
            };

            _serviceMock
              .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
              .ReturnsAsync(new PagedList<RecipeCategoryModel>(
                  categories, new Metadata()));

            var cut = RenderWithMessageComponent();

            var moveDown = typeof(RecipeCategoriesOverview).GetMethod("MoveDown", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(moveDown, Is.Not.Null);

            var firstItem = categories[0];

            cut.InvokeAsync(() => moveDown!.Invoke(cut.Instance, [firstItem]));

            var instanceCategories = cut.Instance.Categories;
            Assert.That(instanceCategories[1].Id, Is.EqualTo(firstItem.Id));
        }
    }
}