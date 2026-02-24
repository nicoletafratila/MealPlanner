using System.Reflection;
using Bunit;
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

            // Loose so we can call methods without setting up every one
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Loose);
            _messageComponentMock = new Mock<IMessageComponent>(MockBehavior.Loose);

            _ctx.Services.AddSingleton(_recipeCategoryServiceMock.Object);
            _ctx.Services.AddSingleton<IMessageComponent>(_messageComponentMock.Object);

            // Needed for Breadcrumb / other BlazorBootstrap services used in the .razor
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

                // Cascading MessageComponent
                parameters.AddCascadingValue("MessageComponent", _messageComponentMock.Object);
            });
        }

        // ---------- OnInitializedAsync ----------
        [Test]
        public void OnInitializedAsync_WithIdZero_CreatesNewRecipeCategory()
        {
            // Act
            var cut = RenderComponent(id: "0");

            // Assert
            Assert.That(cut.Instance.RecipeCategory, Is.Not.Null);
            Assert.That(cut.Instance.RecipeCategory!.Id, Is.EqualTo(0));
            // GetEditAsync must not be called
            _recipeCategoryServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnInitializedAsync_WithNonZeroId_LoadsRecipeCategory()
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

            // Assert
            Assert.That(cut.Instance.RecipeCategory, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.RecipeCategory!.Id, Is.EqualTo(5));
                Assert.That(cut.Instance.RecipeCategory!.Name, Is.EqualTo("Breakfast"));
            });

            _recipeCategoryServiceMock.Verify(s => s.GetEditAsync(5), Times.Once);
        }

        // ---------- SaveAsync guards ----------
        [Test]
        public async Task SaveAsync_DoesNothing_WhenRecipeCategoryIsNull()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.RecipeCategory = null;

            var method = typeof(RecipeCategoryEdit)
                .GetMethod("SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            // Assert: no calls into service
            _recipeCategoryServiceMock.Verify(
                s => s.AddAsync(It.IsAny<RecipeCategoryEditModel>()),
                Times.Never);
            _recipeCategoryServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<RecipeCategoryEditModel>()),
                Times.Never);
        }

        // ---------- DeleteAsync guards ----------
        [Test]
        public async Task DeleteAsync_DoesNothing_WhenRecipeCategoryIsNull()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.RecipeCategory = null;

            var method = typeof(RecipeCategoryEdit)
                .GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
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
        public async Task DeleteAsync_DoesNothing_WhenIdIsZero()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            cut.Instance.RecipeCategory = new RecipeCategoryEditModel { Id = 0 };

            var method = typeof(RecipeCategoryEdit)
                .GetMethod("DeleteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
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

        // ---------- NavigateToOverview ----------
        [Test]
        public void NavigateToOverview_NavigatesToOverviewUrl()
        {
            // Arrange
            var cut = RenderComponent(id: "0");
            var navManager = _ctx.Services.GetRequiredService<NavigationManager>();

            var method = typeof(RecipeCategoryEdit)
                .GetMethod("NavigateToOverview", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            cut.InvokeAsync(() => method!.Invoke(cut.Instance, []));

            // Assert
            Assert.That(navManager.Uri, Does.EndWith("recipebooks/recipecategoriesoverview"));
        }
    }
}