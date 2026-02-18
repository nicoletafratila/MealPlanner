using System.Reflection;
using Bunit;
using Common.Logging;
using Common.Models;
using MealPlanner.UI.Web.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MealPlanner.UI.Web.Tests.Pages
{
    [TestFixture]
    public class AuditTrailTests
    {
        private Mock<ILoggerService> _loggerServiceMock = null!;
        private Mock<ILogger<AuditTrail>> _loggerMock = null!;
        private BunitContext _ctx = null!;

        private static LogModel CreateLog(string message = "Test log", string level = "Information")
        {
            return new LogModel
            {
                Id = 1,
                Message = message,
                MessageTemplate = message,
                Level = level,
                Timestamp = DateTime.UtcNow,
                Exception = string.Empty,
                Properties = "{}"
            };
        }

        [SetUp]
        public void SetUp()
        {
            _ctx = new BunitContext();

            _loggerServiceMock = new Mock<ILoggerService>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<AuditTrail>>(MockBehavior.Loose);

            _loggerServiceMock
                .Setup(s => s.GetLogsAsync())
                .ReturnsAsync([]);

            _ctx.Services.AddSingleton(_loggerServiceMock.Object);
            _ctx.Services.AddSingleton(_loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx.Dispose();
            _loggerServiceMock.Reset();
            _loggerMock.Reset();
        }

        [Test]
        public void OnInitializedAsync_CallsGetLogsAsync_Once()
        {
            // Arrange
            _loggerServiceMock
                .Setup(s => s.GetLogsAsync())
                .ReturnsAsync([])
                .Verifiable();

            // Act
            var cut = _ctx.Render<AuditTrail>();

            // Assert
            _loggerServiceMock.Verify(s => s.GetLogsAsync(), Times.Once);
        }

        [Test]
        public void OnInitializedAsync_LogsError_WhenGetLogsAsyncThrows()
        {
            // Arrange
            _loggerServiceMock
                .Setup(s => s.GetLogsAsync())
                .ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            Assert.DoesNotThrow(() => _ctx.Render<AuditTrail>());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Failed to load audit trail logs.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task DeleteAllLogsAsync_CallsDeleteLogsAsync_AndReloadsLogs()
        {
            // Arrange
            var initialLogs = new List<LogModel> { CreateLog(), CreateLog() };
            var refreshedLogs = new List<LogModel> { CreateLog("Refreshed log") };

            var getLogsCallCount = 0;

            _loggerServiceMock
                .Setup(s => s.GetLogsAsync())
                .ReturnsAsync(() =>
                {
                    getLogsCallCount++;
                    return getLogsCallCount == 1 ? initialLogs : refreshedLogs;
                });

            _loggerServiceMock
                .Setup(s => s.DeleteLogsAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var cut = _ctx.Render<AuditTrail>();

            _loggerServiceMock.Verify(s => s.GetLogsAsync(), Times.Once);

            var method = typeof(AuditTrail)
                .GetMethod("DeleteAllLogsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            _loggerServiceMock.Verify(s => s.DeleteLogsAsync(), Times.Once);
            _loggerServiceMock.Verify(s => s.GetLogsAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task DeleteAllLogsAsync_LogsError_WhenDeleteFails()
        {
            // Arrange
            _loggerServiceMock
                .Setup(s => s.GetLogsAsync())
                .ReturnsAsync([CreateLog()]);

            _loggerServiceMock
                .Setup(s => s.DeleteLogsAsync())
                .ThrowsAsync(new InvalidOperationException("delete failed"));

            var cut = _ctx.Render<AuditTrail>();

            var method = typeof(AuditTrail)
                .GetMethod("DeleteAllLogsAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            await cut.InvokeAsync(async () =>
            {
                var task = (Task)method!.Invoke(cut.Instance, [])!;
                await task;
            });

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) =>
                        v.ToString()!.Contains("Failed to delete audit trail logs.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}