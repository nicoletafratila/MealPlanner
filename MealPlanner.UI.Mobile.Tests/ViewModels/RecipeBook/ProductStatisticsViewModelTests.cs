using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class ProductStatisticsViewModelTests
    {
        private Mock<IStatisticsService> _statisticsServiceMock = null!;
        private Mock<IProductCategoryService> _productCategoryServiceMock = null!;
        private ProductStatisticsViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _statisticsServiceMock = new Mock<IStatisticsService>(MockBehavior.Strict);
            _productCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _viewModel = new ProductStatisticsViewModel(_statisticsServiceMock.Object, _productCategoryServiceMock.Object);
        }

        private void SetupCategorySearch(List<ProductCategoryModel> categories)
        {
            _productCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>(categories, Metadata.Create(1, 500, categories.Count)));
        }

        [Test]
        public async Task LoadAsync_WhenAlreadyBusy_DoesNothing()
        {
            _viewModel.IsBusy = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);

            _productCategoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _statisticsServiceMock.Verify(
                s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            Assert.That(_viewModel.IsBusy, Is.True);
        }

        [Test]
        public async Task LoadAsync_SuccessfulLoad_BuildsCategoryStatisticsWithCorrectMath()
        {
            SetupCategorySearch([]);

            var statistics = new List<StatisticModel>
            {
                new()
                {
                    Title = "Alpha",
                    Data = new Dictionary<string, double?>
                    {
                        ["Apple"] = 30,
                        ["Banana"] = 10,
                        ["Cherry"] = 20
                    }
                },
                new()
                {
                    Title = "Beta",
                    Data = new Dictionary<string, double?>
                    {
                        ["Milk"] = 50,
                        ["Cheese"] = 30
                    }
                }
            };

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync((IList<StatisticModel>?)statistics);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.IsBusy, Is.False);
                Assert.That(_viewModel.ErrorMessage, Is.Null);
                Assert.That(_viewModel.Categories, Has.Count.EqualTo(2));
            }

            var alpha = _viewModel.Categories[0];
            var beta = _viewModel.Categories[1];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(alpha.Title, Is.EqualTo("Alpha"));
                Assert.That(alpha.TotalValue, Is.EqualTo(60).Within(0.0001));
                Assert.That(alpha.SharePercentage, Is.EqualTo(60.0 / 140.0 * 100).Within(0.0001));
                Assert.That(alpha.Items, Has.Count.EqualTo(3));

                Assert.That(alpha.Items[0].Rank, Is.EqualTo(1));
                Assert.That(alpha.Items[0].ItemName, Is.EqualTo("Apple"));
                Assert.That(alpha.Items[0].Value, Is.EqualTo(30));
                Assert.That(alpha.Items[0].BarFraction, Is.EqualTo(1.0).Within(0.0001));

                Assert.That(alpha.Items[1].Rank, Is.EqualTo(2));
                Assert.That(alpha.Items[1].ItemName, Is.EqualTo("Cherry"));
                Assert.That(alpha.Items[1].Value, Is.EqualTo(20));
                Assert.That(alpha.Items[1].BarFraction, Is.EqualTo(20.0 / 30.0).Within(0.0001));

                Assert.That(alpha.Items[2].Rank, Is.EqualTo(3));
                Assert.That(alpha.Items[2].ItemName, Is.EqualTo("Banana"));
                Assert.That(alpha.Items[2].Value, Is.EqualTo(10));
                Assert.That(alpha.Items[2].BarFraction, Is.EqualTo(10.0 / 30.0).Within(0.0001));
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(beta.Title, Is.EqualTo("Beta"));
                Assert.That(beta.TotalValue, Is.EqualTo(80).Within(0.0001));
                Assert.That(beta.SharePercentage, Is.EqualTo(80.0 / 140.0 * 100).Within(0.0001));
                Assert.That(beta.Items, Has.Count.EqualTo(2));

                Assert.That(beta.Items[0].Rank, Is.EqualTo(1));
                Assert.That(beta.Items[0].ItemName, Is.EqualTo("Milk"));
                Assert.That(beta.Items[0].Value, Is.EqualTo(50));
                Assert.That(beta.Items[0].BarFraction, Is.EqualTo(1.0).Within(0.0001));

                Assert.That(beta.Items[1].Rank, Is.EqualTo(2));
                Assert.That(beta.Items[1].ItemName, Is.EqualTo("Cheese"));
                Assert.That(beta.Items[1].Value, Is.EqualTo(30));
                Assert.That(beta.Items[1].BarFraction, Is.EqualTo(30.0 / 50.0).Within(0.0001));
            }
        }

        [Test]
        public async Task LoadAsync_ServiceReturnsNull_SetsCategoriesEmpty()
        {
            SetupCategorySearch([]);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync((IList<StatisticModel>?)null);

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Is.Empty);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_ServiceReturnsEmptyList_SetsCategoriesEmpty()
        {
            SetupCategorySearch([]);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync((IList<StatisticModel>?)new List<StatisticModel>());

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.Categories, Is.Empty);
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }

        [Test]
        public async Task LoadAsync_ServiceThrows_SetsErrorMessage()
        {
            SetupCategorySearch([]);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await _viewModel.LoadCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(_viewModel.IsBusy, Is.False);
            }
        }
    }
}
