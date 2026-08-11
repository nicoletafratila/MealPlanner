using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class ProductsOverviewViewModelTests
    {
        private Mock<IProductService> _productServiceMock = null!;
        private ProductsOverviewViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _productServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _viewModel = new ProductsOverviewViewModel(_productServiceMock.Object);
        }

        [Test]
        public async Task LoadAsync_NoSearchText_PopulatesProductsAndHasNextPage()
        {
            var items = new List<ProductModel>
            {
                new(Guid.NewGuid(), "Milk"),
                new(Guid.NewGuid(), "Bread")
            };
            var metadata = Metadata.Create(1, 20, 40);

            _productServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(p => p.Filters == null && p.PageNumber == 1), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductModel>(items, metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Products, Has.Count.EqualTo(2));
                Assert.That(_viewModel.HasNextPage, Is.True);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_WithSearchText_PassesNameContainsFilter()
        {
            _viewModel.SearchText = "mil";
            var metadata = Metadata.Create(1, 20, 1);

            QueryParameters<ProductModel>? captured = null;
            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None))
                .Callback<QueryParameters<ProductModel>, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync(new PagedList<ProductModel>([new ProductModel(Guid.NewGuid(), "Milk")], metadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.Filters, Is.Not.Null);
                var filter = captured.Filters!.Single();
                Assert.That(filter.PropertyName, Is.EqualTo("Name"));
                Assert.That(filter.Value, Is.EqualTo("mil"));
                Assert.That(filter.Operator, Is.EqualTo(FilterOperator.Contains));
                Assert.That(filter.StringComparison, Is.EqualTo(StringComparison.OrdinalIgnoreCase));
            }
        }

        [Test]
        public async Task LoadAsync_ServiceThrows_SetsErrorMessage()
        {
            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenHasNextPage_AppendsItemsAndIncrementsPage()
        {
            var firstMetadata = Metadata.Create(1, 20, 40);
            _productServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductModel>([new ProductModel(Guid.NewGuid(), "Milk")], firstMetadata));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            var secondMetadata = Metadata.Create(2, 20, 40);
            _productServiceMock
                .Setup(s => s.SearchAsync(It.Is<QueryParameters<ProductModel>>(p => p.PageNumber == 2), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductModel>([new ProductModel(Guid.NewGuid(), "Bread")], secondMetadata));

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Products, Has.Count.EqualTo(2));
                Assert.That(_viewModel.CurrentPage, Is.EqualTo(2));
                Assert.That(_viewModel.IsLoadingMore, Is.False);
            }
        }

        [Test]
        public async Task NextPageAsync_WhenNoNextPage_DoesNotCallService()
        {
            _viewModel.HasNextPage = false;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _productServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task NextPageAsync_WhenIsBusy_DoesNotCallService()
        {
            _viewModel.HasNextPage = true;
            _viewModel.IsBusy = true;

            await _viewModel.NextPageCommand.ExecuteAsync(null);

            _productServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductModel>>(), CancellationToken.None),
                Times.Never);
        }

        [Test]
        public async Task DeleteAsync_Success_RemovesProductFromCollection()
        {
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.Products.Add(product);

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Success());

            await _viewModel.DeleteCommand.ExecuteAsync(product);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Products, Does.Not.Contain(product));
                Assert.That(_viewModel.ErrorMessage, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAsync_Failure_SetsErrorMessageAndKeepsProduct()
        {
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.Products.Add(product);

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id, CancellationToken.None))
                .ReturnsAsync(CommandResponse.Failed("cannot delete"));

            await _viewModel.DeleteCommand.ExecuteAsync(product);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Products, Contains.Item(product));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("cannot delete"));
            }
        }

        [Test]
        public async Task DeleteAsync_ServiceThrows_SetsErrorMessage()
        {
            var product = new ProductModel(Guid.NewGuid(), "Milk");
            _viewModel.Products.Add(product);

            _productServiceMock
                .Setup(s => s.DeleteAsync(product.Id, CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.DeleteCommand.ExecuteAsync(product);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Products, Contains.Item(product));
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }
    }
}
