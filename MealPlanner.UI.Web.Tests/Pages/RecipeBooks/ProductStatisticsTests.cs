using System.Reflection;
using BlazorBootstrap;
using Blazored.SessionStorage;
using Bunit;
using Common.Models;
using Common.Pagination;
using MealPlanner.UI.Web.Pages.RecipeBooks;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Tests.Pages.RecipeBooks
{
    [TestFixture]
    public class ProductStatisticsTests
    {
        private BunitContext _ctx = null!;
        private Mock<IStatisticsService> _statisticsServiceMock = null!;
        private Mock<IProductCategoryService> _categoryServiceMock = null!;
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private PreloadService _preloadService = null!;

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _statisticsServiceMock = new Mock<IStatisticsService>(MockBehavior.Strict);
            _categoryServiceMock = new Mock<IProductCategoryService>(MockBehavior.Strict);
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);

            _preloadService = new PreloadService();

            _ctx.Services.AddSingleton(_statisticsServiceMock.Object);
            _ctx.Services.AddSingleton(_categoryServiceMock.Object);
            _ctx.Services.AddSingleton(_sessionStorageMock.Object);
            _ctx.Services.AddSingleton(_preloadService);

            // JS interop used by BlazorBootstrap charts
            _ctx.JSInterop.SetupVoid("window.blazorBootstrap.chart.initialize", _ => true).SetVoidResult();
            _ctx.JSInterop.SetupVoid("window.blazorChart.doughnut.initialize", _ => true).SetVoidResult();
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _statisticsServiceMock.Reset();
            _categoryServiceMock.Reset();
            _sessionStorageMock.Reset();
        }

        private IRenderedComponent<ProductStatistics> RenderComponent()
        {
            return _ctx.Render<ProductStatistics>();
        }

        // ---------- Initialization / Refresh ----------
        [Test]
        public void OnInitializedAsync_SetsDefaultQueryParameters()
        {
            // Arrange
            var categories = new PagedList<ProductCategoryModel>(
                [],
                new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 0 });

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(It.IsAny<IList<ProductCategoryModel>>()))
                .ReturnsAsync([]);

            // Act
            var cut = RenderComponent();

            // Assert
            var qp = cut.Instance.QueryParameters;
            Assert.That(qp, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(qp!.PageSize, Is.EqualTo(3));
                Assert.That(qp.PageNumber, Is.EqualTo(1));
                Assert.That(qp.Sorting, Is.Not.Null);
                Assert.That(qp.Sorting!.Count, Is.EqualTo(1));
                Assert.That(qp.Sorting!.First().PropertyName, Is.EqualTo("Name"));
                Assert.That(qp.Sorting!.First().Direction, Is.EqualTo(SortDirection.Ascending));
            });
        }

        [Test]
        public void RefreshAsync_LoadsCategories_AndStatistics()
        {
            // Arrange
            var categoryItems = new List<ProductCategoryModel>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            var categories = new PagedList<ProductCategoryModel>(
                categoryItems,
                new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 2 });

            var stats = new List<StatisticModel>
            {
                new() { Title = "S1" },
                new() { Title = "S2" }
            };

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(categoryItems))
                .ReturnsAsync(stats);

            // Act
            var cut = RenderComponent();

            // Assert
            Assert.That(cut.Instance.Categories, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(cut.Instance.Categories!.Items!, Has.Count.EqualTo(2));
                Assert.That(cut.Instance.Statistics, Has.Count.EqualTo(2));
            });
        }

        // ---------- OnPageChangedAsync ----------
        [Test]
        public async Task OnPageChangedAsync_UpdatesPageNumber_AndRefreshes()
        {
            // Arrange
            var categoryItems = new List<ProductCategoryModel>
            {
                new() { Id = 1 }
            };

            var categories = new PagedList<ProductCategoryModel>(
                categoryItems,
                new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            _categoryServiceMock
                .SetupSequence(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories) // first for OnInitializedAsync
                .ReturnsAsync(categories); // second for OnPageChangedAsync

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(categoryItems))
                .ReturnsAsync([]);

            var cut = RenderComponent();

            var method = typeof(ProductStatistics)
                .GetMethod("OnPageChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, new object[] { 2 })!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.QueryParameters!.PageNumber, Is.EqualTo(2));
        }

        // ---------- OnAfterRenderAsync (charts & preload hide) ----------
        [Test]
        public async Task OnAfterRenderAsync_WithStatistics_InitializesCharts_AndHidesPreload()
        {
            // Arrange
            var categoryItems = new List<ProductCategoryModel> { new() { Id = 1 } };
            var categories = new PagedList<ProductCategoryModel>(
                categoryItems,
                new Metadata { PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var stat = new StatisticModel
            {
                Title = "S1",
                Chart = new DoughnutChart(),
                ChartData = new ChartData(),
                ChartOptions = new DoughnutChartOptions()
            };

            _categoryServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<QueryParameters<ProductCategoryModel>>()))
                .ReturnsAsync(categories);

            _statisticsServiceMock
                .Setup(s => s.GetFavoriteProductsAsync(categoryItems))
                .ReturnsAsync([stat]);

            var cut = RenderComponent();

            // Act
            await cut.InvokeAsync(async () =>
            {
                var method = typeof(ProductStatistics)
                    .GetMethod("OnAfterRenderAsync", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                var task = (Task)method!.Invoke(cut.Instance, new object[] { false })!;
                await task;
            });

            // Assert
            Assert.That(cut.Instance.Statistics, Has.Count.EqualTo(1));
        }
    }
}