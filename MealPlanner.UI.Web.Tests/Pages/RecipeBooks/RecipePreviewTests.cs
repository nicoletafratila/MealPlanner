using Bunit;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class RecipePreviewTests
    {
        private BunitContext _ctx = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
        }

        private IRenderedComponent<RecipePreview> RenderComponent(
            RecipeEditModel? recipe = null,
            string? recipeCategory = null)
        {
            return _ctx.Render<RecipePreview>(parameters =>
            {
                if (recipe is not null)
                {
                    parameters.Add(p => p.Recipe, recipe);
                }

                if (recipeCategory is not null)
                {
                    parameters.Add(p => p.RecipeCategory, recipeCategory);
                }
            });
        }

        [Test]
        public void InitialState_WhenNoParameters_AreNull()
        {
            // Act
            var cut = RenderComponent();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.Recipe, Is.Null);
                Assert.That(cut.Instance.RecipeCategory, Is.Null);
            });
        }

        [Test]
        public void Parameters_AreSet_OnInitialRender()
        {
            // Arrange
            var recipe = new RecipeEditModel
            {
                Name = "Test Recipe"
            };
            const string category = "Breakfast";

            // Act
            var cut = RenderComponent(recipe, category);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.Recipe, Is.SameAs(recipe));
                Assert.That(cut.Instance.RecipeCategory, Is.EqualTo(category));
            });
        }

        [Test]
        public void Parameters_CanBeChanged_ByReRendering()
        {
            // Arrange
            var initialRecipe = new RecipeEditModel { Name = "Initial" };
            var updatedRecipe = new RecipeEditModel { Name = "Updated" };

            var cut = RenderComponent(initialRecipe, "Category1");

            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.Recipe, Is.SameAs(initialRecipe));
                Assert.That(cut.Instance.RecipeCategory, Is.EqualTo("Category1"));
            });

            // Act
            cut = RenderComponent(updatedRecipe, "Category2");

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(cut.Instance.Recipe, Is.SameAs(updatedRecipe));
                Assert.That(cut.Instance.RecipeCategory, Is.EqualTo("Category2"));
            });
        }
    }
}