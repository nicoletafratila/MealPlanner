using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class ProductCategoriesViewModelTests
    {
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private ProductCategoriesViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _viewModel = new ProductCategoriesViewModel(_categoryServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_PopulatesCategoriesAndPagination()
        {
            var items = new List<ProductCategoryModel>
            {
                new(Guid.NewGuid(), "Dairy"),
                new(Guid.NewGuid(), "Snacks")
            };
            var metadata = Metadata.Create(1, 200, 400);

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_NoNextPage_SetsHasNextPageFalse()
        {
            var metadata = Metadata.Create(1, 200, 1);

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([new ProductCategoryModel(Guid.NewGuid(), "Dairy")], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            Assert.That(_viewModel.HasNextPage, Is.False);
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 200, 400);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([new ProductCategoryModel(Guid.NewGuid(), "Dairy")], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 200, 400);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ProductCategoryModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([new ProductCategoryModel(Guid.NewGuid(), "Snacks")], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _categoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesCategoryFromCollection()
        {
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            _viewModel.Categories.Add(category);

            _categoryServiceMock
                .Setup(s => s.DeleteAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(category);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Does.Not.Contain(category));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsCategory()
        {
            var category = new ProductCategoryModel(Guid.NewGuid(), "Dairy");
            _viewModel.Categories.Add(category);

            _categoryServiceMock
                .Setup(s => s.DeleteAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(category);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Contains.Item(category));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }
    }
}
