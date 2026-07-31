using Common.Models;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;
using RecipeBook.Shared.Resources;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class ProductCategoryEditViewModelTests
    {
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private ProductCategoryEditViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _viewModel = new ProductCategoryEditViewModel(_categoryServiceMock.Object);
        }

        [Test]
        public void OnCategoryIdChanged_WithEmptyGuid_SetsIsNewTrueAndDoesNotLoad()
        {
            _viewModel.CategoryId = Guid.Empty.ToString();

            Assert.That(_viewModel.IsNew, Is.True);

            _categoryServiceMock.Verify(
                s => s.GetEditAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public void OnCategoryIdChanged_WithNonEmptyGuid_SetsIsNewFalseAndLoadsModel()
        {
            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel(id, "Dairy");

            _categoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);

            _viewModel.CategoryId = id.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsNew, Is.False);
                Assert.That(_viewModel.Model.Id, Is.EqualTo(id));
                Assert.That(_viewModel.Model.Name, Is.EqualTo("Dairy"));
            }
        }

        [Test]
        public async Task SaveAsync_NameMissing_SetsErrorAndDoesNotCallService()
        {
            _viewModel.Model.Name = string.Empty;

            await _viewModel.SaveCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo(RecipeBookSharedMessages.ProductCategoryNameRequired));
                Assert.That(_viewModel.IsBusy, Is.False);
            }

            _categoryServiceMock.Verify(
                s => s.AddAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None),
                Times.Never);
            _categoryServiceMock.Verify(
                s => s.UpdateAsync(It.IsAny<ProductCategoryEditModel>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task SaveAsync_NewCategoryValid_CallsAddAsync()
        {
            _viewModel.CategoryId = Guid.Empty.ToString();
            _viewModel.Model.Name = "Dairy";

            _categoryServiceMock
                .Setup(s => s.AddAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            // Shell.Current.GoToAsync runs after a successful save inside the try/catch, so the
            // resulting NullReferenceException in this test host is swallowed into ErrorMessage.
            // Only the service call is verified here.
            await _viewModel.SaveCommand.ExecuteAsync(null);

            _categoryServiceMock.Verify(s => s.AddAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task SaveAsync_ExistingCategoryValid_CallsUpdateAsync()
        {
            var id = Guid.NewGuid();
            var existing = new ProductCategoryEditModel(id, "Dairy");
            _categoryServiceMock
                .Setup(s => s.GetEditAsync(id, CancellationToken.None))
                .ReturnsAsync(existing);
            _viewModel.CategoryId = id.ToString();

            _viewModel.Model.Name = "Dairy updated";

            _categoryServiceMock
                .Setup(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.SaveCommand.ExecuteAsync(null);

            _categoryServiceMock.Verify(s => s.UpdateAsync(_viewModel.Model, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_WhenIsNew_ReturnsWithoutCallingService()
        {
            // DeleteAsync confirms via Shell.Current.DisplayAlertAsync before any try/catch, so
            // calling it past the IsNew guard would throw in this test host. Only the guard-clause
            // return path is exercised here.
            _viewModel.CategoryId = Guid.Empty.ToString();
            Assert.That(_viewModel.IsNew, Is.True);

            await _viewModel.DeleteCommand.ExecuteAsync(null);

            _categoryServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<Guid>(), CancellationToken.None),
                Times.Never);
        }
    }
}
