using System.Reflection;
using Bunit;
using Common.Models;
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
    public class RecipeCategoryEditTests
    {
        private BunitContext _ctx = null!;
        private Mock<IRecipeCategoryService> _recipeCategoryServiceMock = null!;
        private Mock<IMessageComponent> _messageComponentMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton<IMessageComponent>(_messageComponentMock.Object);

            _ctx.Services.AddBlazorBootstrap();
            _ctx.Services.AddLogging();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<RecipeCategoryEdit> RenderComponent(string? id = null)
        {
            return _ctx.Render<RecipeCategoryEdit>(parameters =>
            {
                if (id is not null)
                {
                    parameters.Add(p => p.Id, id);
                }

                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithNullOrZeroId_CreatesNewRecipeCategory()
        {
            // Act
            var cut = RenderComponent(id: "0");

            // Assert
            Assert.That(cut.Instance.RecipeCategory, Is.Not.Null);
            Assert.That(cut.Instance.RecipeCategory.Id, Is.EqualTo(0));

            _recipeCategoryServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_LoadsRecipeCategory()
        {
            // Arrange
            var existing = new RecipeCategoryEditModel
            {
                Id = 5,
                Name = "Breakfast"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            // Act
            var cut = RenderComponent(id: "5");

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.RecipeCategory.Id, Is.EqualTo(5));
                Assert.That(cut.Instance.RecipeCategory.Name, Is.EqualTo("Breakfast"));
            });

            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_WithValidId_NullFromService_FallsBackToCategoryWithId()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync((RecipeCategoryEditModel?)null);

            // Act
            var cut = RenderComponent(id: "5");

            // Assert
            Assert.That(cut.Instance.RecipeCategory.Id, Is.EqualTo(5));
            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveCoreAsync ----------
        [Test]
        public async Task SaveCoreAsync_AddsRecipeCategory_WhenIdIsZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            _recipeCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new RecipeCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(RecipeCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(
                s => s.AddAsync(It.Is<RecipeCategoryEditModel>(c => c.Name == "New Category")),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipecategoriesoverview"));
        }

        [Test]
        public async Task SaveCoreAsync_UpdatesRecipeCategory_WhenIdIsNonZero()
        {
            // Arrange
            var response = new CommandResponse { Succeeded = true, Message = "ok" };

            var existing = new RecipeCategoryEditModel
            {
                Id = 5,
                Name = "Breakfast"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeCategoryServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<RecipeCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new RecipeCategoryEditModel { Id = 5, Name = "Updated Category" };

            var method = typeof(RecipeCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _recipeCategoryServiceMock.Verify(
                s => s.UpdateAsync(It.Is<RecipeCategoryEditModel>(c => c.Id == 5)),
                Times.Once);

            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been saved successfully"),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsGenericError_WhenResponseIsNull()
        {
            // Arrange
            _recipeCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeCategoryEditModel>()))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "0");

            var category = new RecipeCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(RecipeCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Save failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task SaveCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Validation error"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.AddAsync(It.IsAny<RecipeCategoryEditModel>()))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "0");

            var category = new RecipeCategoryEditModel { Id = 0, Name = "New Category" };

            var method = typeof(RecipeCategoryEdit).GetMethod("SaveCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _messageComponentMock.Verify(
                m => m.ShowError("Validation error"),
                Times.Once);
        }

        // ---------- DeleteAsync / DeleteCoreAsync ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenRecipeCategoryIdIsZero()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.RecipeCategory = new RecipeCategoryEditModel { Id = 0 };

            var method = typeof(RecipeCategoryEdit) .GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task DeleteCoreAsync_Deletes_WhenResponseSucceeded()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = true,
                Message = "ok"
            };

            var existing = new RecipeCategoryEditModel
            {
                Id = 5,
                Name = "Breakfast"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new RecipeCategoryEditModel { Id = 5, Name = "ToDelete" };

            var method = typeof(RecipeCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _recipeCategoryServiceMock.Verify(s => s.DeleteAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowInfo("Data has been deleted successfully"),
                Times.Once);

            var nav = _ctx.Services.GetRequiredService<NavigationManager>();
            Assert.That(nav.Uri, Does.EndWith("recipebooks/recipecategoriesoverview"));
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsGenericError_WhenResponseNull()
        {
            // Arrange
            var existing = new RecipeCategoryEditModel
            {
                Id = 5,
                Name = "Breakfast"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync((CommandResponse?)null);

            var cut = RenderComponent(id: "5");

            var category = new RecipeCategoryEditModel { Id = 5 };

            var method = typeof(RecipeCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed. Please try again."),
                Times.Once);
        }

        [Test]
        public async Task DeleteCoreAsync_ShowsResponseMessage_WhenFailed()
        {
            // Arrange
            var response = new CommandResponse
            {
                Succeeded = false,
                Message = "Delete failed because of dependency"
            };

            var existing = new RecipeCategoryEditModel
            {
                Id = 5,
                Name = "Breakfast"
            };

            _recipeCategoryServiceMock
                .Setup(s => s.GetEditAsync(5))
                .ReturnsAsync(existing);

            _recipeCategoryServiceMock
                .Setup(s => s.DeleteAsync(5))
                .ReturnsAsync(response);

            var cut = RenderComponent(id: "5");

            var category = new RecipeCategoryEditModel { Id = 5 };

            var method = typeof(RecipeCategoryEdit).GetMethod("DeleteCoreAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [category])!;
                await task;
            });

            // Assert
            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once); 
            _messageComponentMock.Verify(
                m => m.ShowError("Delete failed because of dependency"),
                Times.Once);
        }

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(RecipeCategoryEdit).GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(navManager.Uri, Does.EndWith("recipebooks/recipecategoriesoverview"));
        }
    }
}