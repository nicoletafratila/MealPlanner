using Common.Models;
using MealPlanner.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MealPlanner.Api.Tests.Controllers
{
    [TestFixture]
    public class StatisticsControllerTests
    {
        private Mock<ISender> _senderMock = null!;
        private StatisticsController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _senderMock = new Mock<ISender>(MockBehavior.Strict);
            _controller = new StatisticsController(_senderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _senderMock.Reset();
        }

        [Test]
        public async Task SearchFavoriteRecipesAsync_SendsQuery_WithTokenAndCategoryIds()
        {
            // Arrange
            _controller.HttpContext.Request.Headers.Authorization = "Bearer token123";

            var expectedStats = new List<StatisticModel>
            {
                new() { Title = "S1" },
                new() { Title = "S2" }
            };

            Api.Features.Statistics.Queries.SearchRecipes.SearchQuery? capturedQuery = null;

            _senderMock
                .Setup(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchRecipes.SearchQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IRequest<IList<StatisticModel>>, CancellationToken>((q, _) =>
                {
                    capturedQuery = (Api.Features.Statistics.Queries.SearchRecipes.SearchQuery)q;
                })
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.SearchFavoriteRecipesAsync("1,2", CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(expectedStats));

                Assert.That(capturedQuery, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery!.CategoryIds, Is.EqualTo("1,2"));
                Assert.That(capturedQuery!.AuthToken, Is.EqualTo("token123"));
            });

            _senderMock.Verify(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchRecipes.SearchQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SearchFavoriteProductsAsync_SendsQuery_WithTokenAndCategoryIds()
        {
            // Arrange
            _controller.HttpContext.Request.Headers.Authorization = "Bearer tok";

            var expectedStats = new List<StatisticModel>
            {
                new() { Title = "P1" }
            };

            Api.Features.Statistics.Queries.SearchProducts.SearchQuery? capturedQuery = null;

            _senderMock
                .Setup(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchProducts.SearchQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IRequest<IList<StatisticModel>?>, CancellationToken>((q, _) =>
                {
                    capturedQuery = (Api.Features.Statistics.Queries.SearchProducts.SearchQuery)q;
                })
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.SearchFavoriteProductsAsync("3,4", CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(expectedStats));

                Assert.That(capturedQuery, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery!.CategoryIds, Is.EqualTo("3,4"));
                Assert.That(capturedQuery!.AuthToken, Is.EqualTo("tok"));
            });

            _senderMock.Verify(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchProducts.SearchQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SearchFavoriteRecipesAsync_WithoutAuthHeader_UsesNullToken()
        {
            // Arrange
            // no Authorization header
            var expectedStats = new List<StatisticModel>();

            Api.Features.Statistics.Queries.SearchRecipes.SearchQuery? capturedQuery = null;

            _senderMock
                .Setup(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchRecipes.SearchQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IRequest<IList<StatisticModel>>, CancellationToken>((q, _) =>
                {
                    capturedQuery = (Api.Features.Statistics.Queries.SearchRecipes.SearchQuery)q;
                })
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _controller.SearchFavoriteRecipesAsync(null, CancellationToken.None);

            // Assert
            var ok = result.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(ok!.Value, Is.SameAs(expectedStats));

                Assert.That(capturedQuery, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery!.CategoryIds, Is.Null);
                Assert.That(capturedQuery!.AuthToken, Is.Empty);
            });

            _senderMock.Verify(m => m.Send(
                    It.IsAny<Api.Features.Statistics.Queries.SearchRecipes.SearchQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}