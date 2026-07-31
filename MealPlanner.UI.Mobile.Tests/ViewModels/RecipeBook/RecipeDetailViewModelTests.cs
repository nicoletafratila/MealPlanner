using System.Reflection;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class RecipeDetailViewModelTests
    {
        private Mock<IRecipeService> _recipeServiceMock = null!;
        private RecipeDetailViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _recipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);
            _viewModel = new RecipeDetailViewModel(_recipeServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_RecipeIdIsEmptyGuid_DoesNotCallServiceAndLeavesRecipeNull()
        {
            _viewModel.RecipeId = Guid.Empty.ToString();

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipe, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task LoadAsync_RecipeIdIsNotAValidGuid_DoesNotCallService()
        {
            _viewModel.RecipeId = "not-a-guid";

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipe, Is.Null);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.GetEditAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task OnRecipeIdChanged_ValidId_TriggersLoadAndPopulatesRecipe()
        {
            var id = Guid.NewGuid();
            var model = new RecipeEditModel(id, "Pasta", Guid.NewGuid());

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(model);

            _viewModel.RecipeId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipe, Is.SameAs(model));
                Assert.That(_viewModel.IsBusy, Is.False);
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task LoadAsync_ValidId_PopulatesRecipe()
        {
            var id = Guid.NewGuid();
            var model = new RecipeEditModel(id, "Pasta", Guid.NewGuid());

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(model);

            // Set the backing field directly (bypasses OnRecipeIdChanged) so LoadCommand
            // is invoked exactly once for this assertion.
            SetRecipeIdBackingField(id.ToString());
            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Recipe, Is.SameAs(model));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
            _recipeServiceMock.Verify(s => s.GetEditAsync(id, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task LoadAsync_ServiceThrows_SetsErrorMessage()
        {
            var id = Guid.NewGuid();

            _recipeServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            SetRecipeIdBackingField(id.ToString());
            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        private void SetRecipeIdBackingField(string value)
        {
            var field = typeof(RecipeDetailViewModel).GetField("_recipeId", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Backing field '_recipeId' not found.");
            field.SetValue(_viewModel, value);
        }
    }
}
