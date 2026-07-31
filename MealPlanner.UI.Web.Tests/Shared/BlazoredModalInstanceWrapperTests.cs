using System.Reflection;
using Blazored.Modal;
using Bunit;
using Common.Pagination;
using MealPlanner.UI.Web.Pages;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Shared;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Shared
{
    [TestFixture]
    public class BlazoredModalInstanceWrapperTests
    {
        private BunitContext _ctx = null!;
        private Mock<IRecipeCategoryService> _categoryServiceMock = null!;
        private Mock<IRecipeService> _recipeServiceMock = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _categoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>()))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([], new Metadata()));

            _ctx.Services.AddSingleton(_categoryServiceMock.Object);
            _ctx.Services.AddSingleton(_recipeServiceMock.Object);
            _ctx.Services.AddBlazoredModal();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _categoryServiceMock.Reset();
            _recipeServiceMock.Reset();
        }

        private IRenderedComponent<BlazoredModalInstanceWrapper> RenderComponent(BlazoredModalInstance modalInstance)
        {
            return _ctx.Render<BlazoredModalInstanceWrapper>(parameters =>
                parameters.AddCascadingValue(modalInstance));
        }

        [Test]
        public void OnInitialized_CreatesModalController_WrappingCascadedInstance()
        {
            // Arrange
            var modalInstance = new BlazoredModalInstance();

            // Act
            var cut = RenderComponent(modalInstance);

            // Assert
            var controller = typeof(BlazoredModalInstanceWrapper)
                .GetField("_controller", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(cut.Instance);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller, Is.InstanceOf<BlazoredModalController>());
        }

        [Test]
        public void Renders_RecipeSelection_AsChildComponent()
        {
            // Arrange
            var modalInstance = new BlazoredModalInstance();

            // Act
            var cut = RenderComponent(modalInstance);

            // Assert
            Assert.That(cut.FindComponent<RecipeSelection>(), Is.Not.Null);
        }
    }
}
