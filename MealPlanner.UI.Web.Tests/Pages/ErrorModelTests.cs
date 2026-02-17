using System.Diagnostics;
using MealPlanner.UI.Web.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages
{
    [TestFixture]
    public class ErrorModelTests
    {
        private Mock<ILogger<ErrorModel>> _loggerMock = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<ErrorModel>>(MockBehavior.Loose);
        }

        private static ErrorModel CreateModel(ILogger<ErrorModel> logger, HttpContext? httpContext = null)
        {
            var model = new ErrorModel(logger)
            {
                PageContext = new PageContext
                {
                    HttpContext = httpContext ?? new DefaultHttpContext()
                }
            };
            return model;
        }

        [Test]
        public void ShowRequestId_IsFalse_WhenRequestIdIsNullOrEmpty()
        {
            // Arrange
            var model = CreateModel(_loggerMock.Object);

            // Act/Assert
            Assert.That(model.RequestId, Is.Null);
            Assert.That(model.ShowRequestId, Is.False);
        }

        [Test]
        public void OnGet_SetsRequestId_FromActivityCurrent_WhenAvailable()
        {
            // Arrange
            var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-123" };
            var model = CreateModel(_loggerMock.Object, httpContext);

            var activity = new Activity("test-activity");
            activity.Start();
            try
            {
                var expectedId = activity.Id;

                // Act
                model.OnGet();

                // Assert
                Assert.That(model.RequestId, Is.EqualTo(expectedId));
                Assert.That(model.ShowRequestId, Is.True);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, _) =>
                            v.ToString()!.Contains("Unhandled error occurred")),
                        It.IsAny<Exception?>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                activity.Stop();
                Activity.Current = null;
            }
        }

        [Test]
        public void OnGet_SetsRequestId_FromHttpContext_WhenNoActivity()
        {
            // Arrange
            Activity.Current = null;
            var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-xyz" };
            var model = CreateModel(_loggerMock.Object, httpContext);

            // Act
            model.OnGet();

            // Assert
            Assert.That(model.RequestId, Is.EqualTo("trace-xyz"));
            Assert.That(model.ShowRequestId, Is.True);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Unhandled error occurred")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
