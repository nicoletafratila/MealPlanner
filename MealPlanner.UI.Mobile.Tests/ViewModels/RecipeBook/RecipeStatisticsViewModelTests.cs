using Common.Models;
using Common.Pagination;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels.RecipeBook;
using Moq;
using RecipeBook.Services.Http;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Mobile.Tests.ViewModels.RecipeBook
{
    [TestFixture]
    public class RecipeStatisticsViewModelTests
    {
        private Mock<IStatisticsService> _statisticsServiceMock = null!;
        private Mock<IRecipeCategoryService> _recipeCategoryServiceMock = null!;
        private Mock<IUnitService> _lookupUnitServiceMock = null!;
        private Mock<IProductCategoryService> _lookupProductCategoryServiceMock = null!;
        private Mock<IShopService> _lookupShopServiceMock = null!;
        private Mock<IProductService> _lookupProductServiceMock = null!;
        private Mock<IRecipeService> _lookupRecipeServiceMock = null!;
        private RecipeStatisticsViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _statisticsServiceMock = new Mock<IStatisticsService>(MockBehavior.Strict);
            _recipeCategoryServiceMock = new Mock<IRecipeCategoryService>(MockBehavior.Strict);
            _lookupUnitServiceMock = new Mock<IUnitService>(MockBehavior.Strict);
            _lookupProductCategoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _lookupShopServiceMock = new Mock<IShopService>(MockBehavior.Strict);
            _lookupProductServiceMock = new Mock<IProductService>(MockBehavior.Strict);
            _lookupRecipeServiceMock = new Mock<IRecipeService>(MockBehavior.Strict);

            var lookupDataService = new ReferenceDataCacheService(
                _recipeCategoryServiceMock.Object,
                _lookupUnitServiceMock.Object,
                _lookupProductServiceMock.Object,
                _lookupProductCategoryServiceMock.Object,
                _lookupShopServiceMock.Object,
                _lookupRecipeServiceMock.Object);

            _viewModel = new RecipeStatisticsViewModel(_statisticsServiceMock.Object, lookupDataService);
        }

        private void SetupCategorySearch(List<RecipeCategoryModel> categories)
        {
            _recipeCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<RecipeCategoryModel>(categories, Metadata.Create(1, 100, categories.Count)));
            _lookupUnitServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<UnitModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<UnitModel>([], Metadata.Create(1, 100, 0)));
            _lookupProductCategoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ProductCategoryModel>([], Metadata.Create(1, 200, 0)));
            _lookupShopServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ShopModel>>(), CancellationToken.None))
                .ReturnsAsync(new PagedList<ShopModel>([], Metadata.Create(1, 200, 0)));
        }

        [Test]
        public async Task LoadAsync_WhenAlreadyBusy_DoesNothing()
        {
            _viewModel.IsBusy = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);

            _recipeCategoryServiceMock.Verify(
                s => s.SearchAsync(It.IsAny<QueryParameters<RecipeCategoryModel>>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _statisticsServiceMock.Verify(
                s => s.GetFavoriteRecipesAsync(It.IsAny<IList<RecipeCategoryModel>>(), It.IsAny<CancellationToken>()),
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
                        ["Pasta"] = 30,
                        ["Soup"] = 10,
                        ["Salad"] = 20
                    }
                },
                new()
                {
                    Title = "Beta",
                    Data = new Dictionary<string, double?>
                    {
                        ["Cake"] = 50,
                        ["Pie"] = 30
                    }
                }
            };

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteRecipesAsync(It.IsAny<IList<RecipeCategoryModel>>(), CancellationToken.None))
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
                Assert.That(alpha.Items[0].ItemName, Is.EqualTo("Pasta"));
                Assert.That(alpha.Items[0].Value, Is.EqualTo(30));
                Assert.That(alpha.Items[0].BarFraction, Is.EqualTo(1.0).Within(0.0001));

                Assert.That(alpha.Items[1].Rank, Is.EqualTo(2));
                Assert.That(alpha.Items[1].ItemName, Is.EqualTo("Salad"));
                Assert.That(alpha.Items[1].Value, Is.EqualTo(20));
                Assert.That(alpha.Items[1].BarFraction, Is.EqualTo(20.0 / 30.0).Within(0.0001));

                Assert.That(alpha.Items[2].Rank, Is.EqualTo(3));
                Assert.That(alpha.Items[2].ItemName, Is.EqualTo("Soup"));
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
                Assert.That(beta.Items[0].ItemName, Is.EqualTo("Cake"));
                Assert.That(beta.Items[0].Value, Is.EqualTo(50));
                Assert.That(beta.Items[0].BarFraction, Is.EqualTo(1.0).Within(0.0001));

                Assert.That(beta.Items[1].Rank, Is.EqualTo(2));
                Assert.That(beta.Items[1].ItemName, Is.EqualTo("Pie"));
                Assert.That(beta.Items[1].Value, Is.EqualTo(30));
                Assert.That(beta.Items[1].BarFraction, Is.EqualTo(30.0 / 50.0).Within(0.0001));
            }
        }

        [Test]
        public async Task LoadAsync_ServiceReturnsNull_SetsCategoriesEmpty()
        {
            SetupCategorySearch([]);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteRecipesAsync(It.IsAny<IList<RecipeCategoryModel>>(), CancellationToken.None))
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
                .Setup(s => s.GetFavoriteRecipesAsync(It.IsAny<IList<RecipeCategoryModel>>(), CancellationToken.None))
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
                .Setup(s => s.GetFavoriteRecipesAsync(It.IsAny<IList<RecipeCategoryModel>>(), CancellationToken.None))
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
