using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class RecipeCategoriesViewModelTests
    {
        private Mock<IRecipeCategoryService> _categoryServiceMock = null!;
        private RecipeCategoriesViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _viewModel = new RecipeCategoriesViewModel(_categoryServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_PopulatesCategoriesAndPagination()
        {
            var items = new List<RecipeCategoryModel>
            {
                new(Guid.NewGuid(), "Desert", 1),
                new(Guid.NewGuid(), "Fel principal", 2)
            };
            var metadata = Metadata.Create(1, 200, 400);

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>(items, metadata));

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
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([new RecipeCategoryModel(Guid.NewGuid(), "Desert", 1)], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            Assert.That(_viewModel.HasNextPage, Is.False);
        }

        [Test]
        public async Task SearchText_ClearedAfterSearch_ReloadsAllCategories()
        {
            var metadata = Metadata.Create(1, 200, 2);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeCategoryModel>>(p => p.Filters == null), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>(
                    [new RecipeCategoryModel(Guid.NewGuid(), "Desert", 1), new RecipeCategoryModel(Guid.NewGuid(), "Fel principal", 2)], metadata));

            _viewModel.SearchText = string.Empty;
            if (_viewModel.SearchCommand.ExecutionTask is { } task)
            {
                await task;
            }

            Assert.That(_viewModel.Categories, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 200, 400);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([new RecipeCategoryModel(Guid.NewGuid(), "Desert", 1)], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 200, 400);
            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<RecipeCategoryModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>([new RecipeCategoryModel(Guid.NewGuid(), "Fel principal", 2)], secondMetadata));

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
                s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesCategoryFromCollection()
        {
            var category = new RecipeCategoryModel(Guid.NewGuid(), "Desert", 1);
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
            var category = new RecipeCategoryModel(Guid.NewGuid(), "Desert", 1);
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
